using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Keiwando.Evolution.Scenes {

    public abstract class BaseStructure: IStructure {

        public Transform Transform { get; private set; }

        public BaseStructure(Transform transform) {
            this.Transform = transform;
        }

        abstract public string GetEncodingKey();
        public abstract IStructureBuilder GetBuilder();

        
        public virtual JObject Encode() {
            var json = new JObject();
            json[CodingKey.Transform] = JToken.FromObject(this.Transform);
            return json;
        }

        protected static Transform DecodeTransform(JObject json) {
            return json[CodingKey.Transform].ToObject<Transform>();
        }

        private static class CodingKey {
            public const string Transform = "transform";
        }
    }
}