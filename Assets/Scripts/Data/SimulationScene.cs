using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Keiwando.Evolution.Scenes {

    public class SimulationScene {

        public delegate IStructure DecodeStructure(JObject encoded);

        private static Dictionary<string, DecodeStructure> registeredStructures 
            = new Dictionary<string, DecodeStructure>();

        public static void RegisterStructure(string encodingID, DecodeStructure decode) {
            if (registeredStructures.ContainsKey(encodingID)) {
                throw new System.ArgumentException(string.Format("encodingID not unique! {0} has already been registered for a different structure type.", encodingID));
            }
            registeredStructures[encodingID] = decode;
        }

        public Vector3 SpawnPoint;

        public IStructure[] Structures;

        #region Encode & Decode

        private static class CodingKey {
            public const string SpawnPoint = "spawnPoint";
            public const string Structures = "structures";
            public const string EncodingID = "encodingID";
            public const string StructureData = "structureData";
        }

        public string Encode() {

            var sceneJSON = new JObject();
            sceneJSON[CodingKey.SpawnPoint] = JToken.FromObject(SpawnPoint);

            var structures = new JObject[Structures.Length];
            for (int i = 0; i < structures.Length; i++) {
                var structureContainer = new JObject();
                structureContainer[CodingKey.EncodingID] = Structures[i].GetEncodingKey();
                structureContainer[CodingKey.StructureData] = Structures[i].Encode();
                structures[i] = structureContainer;
            }

            sceneJSON[CodingKey.Structures] = JToken.FromObject(structures);
            return sceneJSON.ToString();
        }

        public static SimulationScene Decode(string encoded) {

            JObject json = JObject.Parse(encoded);
            
            var spawnPoint = json[CodingKey.SpawnPoint].ToObject<Vector3>();
            var encodedStructures = json[CodingKey.Structures].ToObject<List<JObject>>();
            var structures = new IStructure[encodedStructures.Count];
            
            for (int i = 0; i < encodedStructures.Count; i++) {
                var structureContainer = encodedStructures[i];
                var encodingID = structureContainer[CodingKey.EncodingID].ToString();
                if (registeredStructures.ContainsKey(encodingID)) {
                    Debug.LogError(string.Format("Structure with encodingID {0} cannot be decoded!", encodingID));
                    continue;
                }
                var decodingFunc = registeredStructures[encodingID];
                var encodedStructure = structureContainer[CodingKey.StructureData].ToObject<JObject>();
                structures[i] = decodingFunc(encodedStructure);
            }

            return new SimulationScene() {
                SpawnPoint = spawnPoint,
                Structures = structures
            };
        }

        #endregion
    }
}



