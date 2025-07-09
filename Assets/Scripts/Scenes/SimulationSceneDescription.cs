using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Keiwando.JSON;

namespace Keiwando.Evolution.Scenes {

    public class SimulationSceneDescription {

        private const int LATEST_JSON_VERSION = 1;
        private const ushort LATEST_BINARY_VERSION = 1;

        public int Version = LATEST_JSON_VERSION;
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

        public void Encode(BinaryWriter writer) {
            // version
            writer.Write((ushort)LATEST_BINARY_VERSION);
            long dataLengthOffset = writer.Seek(0, SeekOrigin.Current);
            writer.WriteDummyBlockLength();

            writer.Write(this.Version);
            writer.Write(this.CameraControlPoints.Length); 
            foreach (CameraControlPoint controlPoint in this.CameraControlPoints) {
                controlPoint.Encode(writer);
            }
            writer.Write(this.DropHeight);
            this.PhysicsConfiguration.Encode(writer);

            writer.Write(this.Structures.Length);
            foreach (IStructure structure in Structures) {
                long structureLengthOffset = writer.Seek(0, SeekOrigin.Current);
                writer.WriteDummyBlockLength();

                StructureType structureType = structure.GetStructureType();
                writer.Write((ushort)structureType);

                switch (structureType) {
                    case StructureType.Ground: {
                        Ground ground = (Ground)structure;
                        ground.Encode(writer);
                        break;
                    }
                    case StructureType.Wall: {
                        Wall wall = (Wall)structure;
                        wall.Encode(writer);
                        break;
                    }
                    case StructureType.DistanceMarkerSpawner: {
                        DistanceMarkerSpawner spawner = (DistanceMarkerSpawner)structure;
                        spawner.Encode(writer);
                        break;
                    }
                    case StructureType.RollingObstacleSpawner: {
                        RollingObstacleSpawner spawner = (RollingObstacleSpawner)structure;
                        spawner.Encode(writer);
                        break;
                    }
                    case StructureType.Stairstep: {
                        Stairstep stairstep = (Stairstep)structure;
                        stairstep.Encode(writer);
                        break;
                    }
                }

                writer.WriteBlockLengthToOffset(structureLengthOffset);
            }
            
            writer.WriteBlockLengthToOffset(dataLengthOffset);
        }

        public static SimulationSceneDescription Decode(BinaryReader reader) {
            int binaryVersion = reader.ReadUInt16();
            if (binaryVersion > LATEST_BINARY_VERSION) {
                return null;
            }
            uint dataLength = reader.ReadBlockLength();
            long expectedSceneDescriptionEndByte = reader.BaseStream.Position + (long)dataLength;
            int version = reader.ReadInt32();
            int cameraControlPointCount = reader.ReadInt32();
            CameraControlPoint[] cameraControlPoints = new CameraControlPoint[cameraControlPointCount];
            for (int i = 0; i < cameraControlPointCount; i++) {
                CameraControlPoint controlPoint = CameraControlPoint.Decode(reader);
                cameraControlPoints[i] = controlPoint;
            }
            float dropHeight = reader.ReadSingle();
            ScenePhysicsConfiguration physicsConfiguration = ScenePhysicsConfiguration.Decode(reader);
            if (physicsConfiguration == null) {
                return null;
            }

            int structuresCount = reader.ReadInt32();
            IStructure[] structures = new IStructure[structuresCount];
            for (int i = 0; i < structuresCount; i++) {
                uint structureDataLength = reader.ReadBlockLength();
                long expectedStructureEndByte = reader.BaseStream.Position + (long)structureDataLength;

                ushort rawStructureType = reader.ReadUInt16();
                switch (rawStructureType) {
                    case (ushort)StructureType.Ground: {
                        structures[i] = Ground.Decode(reader);
                        break;
                    }
                    case (ushort)StructureType.Wall: {
                        structures[i] = Wall.Decode(reader);
                        break;
                    }
                    case (ushort)StructureType.DistanceMarkerSpawner: {
                        structures[i] = DistanceMarkerSpawner.Decode(reader);
                        break;
                    }
                    case (ushort)StructureType.RollingObstacleSpawner: {
                        structures[i] = RollingObstacleSpawner.Decode(reader);
                        break;
                    }
                    case (ushort)StructureType.Stairstep: {
                        structures[i] = Stairstep.Decode(reader);
                        break;
                    }
                    default: {
                        // Unknown structure => skip
                        break;
                    }
                }

                reader.BaseStream.Seek(expectedStructureEndByte, SeekOrigin.Begin);
            }
            
            reader.BaseStream.Seek(expectedSceneDescriptionEndByte, SeekOrigin.Begin);

            return new SimulationSceneDescription(
                version: version, 
                structures: structures, 
                dropHeight: dropHeight, 
                physicsConfiguration: physicsConfiguration, 
                controlPoints: cameraControlPoints
            );
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



