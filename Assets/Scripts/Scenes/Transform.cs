using UnityEngine;

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
    }
}