using Keiwando.JSON;

using UnityEngine;

namespace Keiwando.Evolution.Scenes {

    public abstract class BaseStructure: IStructure {

        public Transform Transform { get; private set; }

        public BaseStructure(Transform transform) {
            this.Transform = transform;
        }

        abstract public string GetEncodingKey();
        public abstract IStructureBuilder GetBuilder();

        #region Encode & Decode

        private static class CodingKey {
            public const string Transform = "transform";
        }

        public virtual JObject Encode() {
            var json = new JObject();
            json[CodingKey.Transform] = this.Transform.Encode();
            return json;
        }

        protected static Transform DecodeTransform(JObject json) {
            return Transform.Decode(json[CodingKey.Transform] as JObject);
        }

        #endregion
    }
}