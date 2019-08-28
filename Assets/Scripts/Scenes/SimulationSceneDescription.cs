using System;
using System.Collections.Generic;
using UnityEngine;
using Keiwando.JSON;

namespace Keiwando.Evolution.Scenes {

    // public class RegisterInSceneAttribute: Attribute {
    //     public readonly string id;
    //     public RegisterInSceneAttribute(string id) {
    //         this.id = id;
    //     }
    // }

    public class SimulationSceneDescription {

        public const int LATEST_VERSION = 1;

        public int Version = LATEST_VERSION;
        public IStructure[] Structures = new IStructure[0];
        public float DropHeight = 0.5f;
        public ScenePhysicsConfiguration PhysicsConfiguration = new ScenePhysicsConfiguration();
        public CameraControlPoint[] CameraControlPoints = new [] { 
            new CameraControlPoint(0, 0, 0.5f), new CameraControlPoint(0, 0, 0.5f)    
        };

        public SimulationSceneDescription() {}
        
        public SimulationSceneDescription(
            int version, IStructure[] structures, float dropHeight, 
            ScenePhysicsConfiguration physicsConfiguration,
            CameraControlPoint[] controlPoints
        ) {
            this.Version = version;
            this.Structures = structures;
            this.DropHeight = dropHeight;
            this.PhysicsConfiguration = physicsConfiguration;
            this.CameraControlPoints = controlPoints;
        }

        #region Encode & Decode

        public delegate IStructure DecodeStructure(JObject encoded);

        private static Dictionary<string, DecodeStructure> registeredStructures 
            = new Dictionary<string, DecodeStructure>();

        public static void RegisterStructure(string encodingID, DecodeStructure decode) {
            if (registeredStructures.ContainsKey(encodingID)) {
                throw new System.ArgumentException(string.Format("encodingID not unique! {0} has already been registered for a different structure type.", encodingID));
            }
            registeredStructures[encodingID] = decode;
        }

        static SimulationSceneDescription() {

            var allStructures = StructureRegistry.Structures;
            for (int i = 0; i < allStructures.Length; i++) {
                var structure = allStructures[i];
                RegisterStructure(structure.id, structure.decoder);
            }
        }

        private static class CodingKey {
            public const string Structures = "structures";
            public const string EncodingID = "encodingID";
            public const string StructureData = "structureData";
            public const string Version = "version";
            public const string DropHeight = "dropHeight";
            public const string PhysicsConfig = "physicsConfiguration";
            public const string CameraControlPoints = "cameraControlPoints";
        }

        public JObject Encode() {

            var json = new JObject();
            // Version
            json[CodingKey.Version] = this.Version;
            // Camera Control Points
            var controlPoints = new JObject[this.CameraControlPoints.Length];
            for (int i = 0; i < controlPoints.Length; i++) {
                controlPoints[i] = this.CameraControlPoints[i].Encode();
            }
            json[CodingKey.CameraControlPoints] = new JArray(controlPoints);
            // Drop Height
            json[CodingKey.DropHeight] = this.DropHeight;
            // Physics Configuration
            json[CodingKey.PhysicsConfig] = this.PhysicsConfiguration.Encode();
            // Structures
            var structures = new JObject[Structures.Length];
            for (int i = 0; i < structures.Length; i++) {
                var structureContainer = new JObject();
                structureContainer[CodingKey.EncodingID] = Structures[i].GetEncodingKey();
                structureContainer[CodingKey.StructureData] = Structures[i].Encode();
                structures[i] = structureContainer;
            }
            json[CodingKey.Structures] = new JArray(structures);

            return json;
        }

        public static SimulationSceneDescription Decode(string encoded) {

            JObject json = JObject.Parse(encoded);
            return Decode(json);
        }

        public static SimulationSceneDescription Decode(JObject json) {

            // Version
            int version = json[CodingKey.Version].ToInt();
            // Camera Control Points
            var controlPoints = json[CodingKey.CameraControlPoints].ToArray(CameraControlPoint.Decode);
            // Drop Height
            var dropHeight = json[CodingKey.DropHeight].ToFloat();
            // Physics Config
            var physicsConfig = ScenePhysicsConfiguration.Decode(json.ObjectForKey(CodingKey.PhysicsConfig));
            // Structures
            var encodedStructures = json[CodingKey.Structures].ToList();
            var structures = new IStructure[encodedStructures.Count];
            
            for (int i = 0; i < encodedStructures.Count; i++) {
                var structureContainer = encodedStructures[i];
                var encodingID = structureContainer[CodingKey.EncodingID].ToString();
                if (!registeredStructures.ContainsKey(encodingID)) {
                    Debug.LogError(string.Format("Structure with encodingID {0} cannot be decoded!", encodingID));
                    continue;
                }
                var decodingFunc = registeredStructures[encodingID];
                var encodedStructure = structureContainer[CodingKey.StructureData] as JObject;
                structures[i] = decodingFunc(encodedStructure);
            }

            return new SimulationSceneDescription(version, structures, dropHeight, physicsConfig, controlPoints);
        }

        #endregion
    }
}



