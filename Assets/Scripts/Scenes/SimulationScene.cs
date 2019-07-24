using System;
using System.Collections.Generic;
using UnityEngine;
using Keiwando.JSON;

namespace Keiwando.Evolution.Scenes {

    public class RegisterInSceneAttribute: Attribute {
        public readonly string id;
        public RegisterInSceneAttribute(string id) {
            this.id = id;
        }
    }

    public class SimulationSceneDescription {

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
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (Type type in assembly.GetTypes()) {
                    var attributes = type.GetCustomAttributes(typeof(RegisterInSceneAttribute), true);
                    if (type.GetCustomAttributes(typeof(RegisterInSceneAttribute), true).Length > 0) {
                    var attribute = attributes[0] as RegisterInSceneAttribute;
                        RegisterStructure(
                            attribute.id, 
                            Delegate.CreateDelegate(
                                typeof(DecodeStructure),
                                type.GetMethod("Decode"),
                                true
                            ) as DecodeStructure
                        ); 
                    }
                }
            }
        }

        public IStructure[] Structures;

        #region Encode & Decode

        private static class CodingKey {
            public const string Structures = "structures";
            public const string EncodingID = "encodingID";
            public const string StructureData = "structureData";
        }

        public JObject Encode() {

            var sceneJSON = new JObject();

            var structures = new JObject[Structures.Length];
            for (int i = 0; i < structures.Length; i++) {
                var structureContainer = new JObject();
                structureContainer[CodingKey.EncodingID] = Structures[i].GetEncodingKey();
                structureContainer[CodingKey.StructureData] = Structures[i].Encode();
                structures[i] = structureContainer;
            }

            sceneJSON[CodingKey.Structures] = new JArray(structures);
            return sceneJSON;
        }

        public static SimulationSceneDescription Decode(string encoded) {

            JObject json = JObject.Parse(encoded);
            return Decode(json);
        }

        public static SimulationSceneDescription Decode(JObject json) {

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

            return new SimulationSceneDescription() {
                Structures = structures
            };
        }

        #endregion
    }
}



