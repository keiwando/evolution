using UnityEngine;
using Keiwando.JSON;

namespace Keiwando.Evolution.Scenes {

    public readonly struct Transform {
            
        public readonly Vector3 Position;
        public readonly float Rotation;
        public readonly Vector3 Scale;

        public Transform(Vector3 pos): this(pos, 0f) {}
        public Transform(Vector3 pos, Vector3 scale): this(pos, 0f, scale) {}
        public Transform(Vector3 pos, float rotation): this(pos, rotation, Vector3.one) {}
        public Transform(Vector3 pos, float rotation, Vector3 scale) {
            this.Position = pos;
            this.Rotation = rotation;
            this.Scale = scale;
        }

        #region Encode & Decode

        private static class CodingKey {
            public const string X = "x";
            public const string Y = "y";
            public const string Z = "z";
            public const string Rotation = "rotation";
            public const string ScaleX = "sX";
            public const string ScaleY = "sY";
            public const string ScaleZ = "sZ";
        }

        public JObject Encode() {

            JObject json = new JObject();
            json[CodingKey.X] = this.Position.x;
            json[CodingKey.Y] = this.Position.y;
            json[CodingKey.Z] = this.Position.z;
            json[CodingKey.Rotation] = this.Rotation;
            json[CodingKey.ScaleX] = this.Scale.x;
            json[CodingKey.ScaleY] = this.Scale.y;
            json[CodingKey.ScaleZ] = this.Scale.z;
            return json;
        }

        public static Transform Decode(JObject json) {
            
            float x = json[CodingKey.X].ToFloat();
            float y = json[CodingKey.Y].ToFloat();
            float z = json[CodingKey.Z].ToFloat();
            float rotation = json[CodingKey.Rotation].ToFloat();
            float sX = json[CodingKey.ScaleX].ToFloat();
            float sY = json[CodingKey.ScaleY].ToFloat();
            float sZ = json[CodingKey.ScaleZ].ToFloat();
            return new Transform(
                new Vector3(x, y, z),
                rotation, 
                new Vector3(sX, sY, sZ)
            );
        }

        #endregion
    }
}