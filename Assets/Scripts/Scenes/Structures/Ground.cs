using System.IO;
using Keiwando.JSON;

using UnityEngine;

namespace Keiwando.Evolution.Scenes {

    public class Ground: BaseStructure {

        public const string ENCODING_ID = "evolution::structure::ground";

        public Ground(Transform transform): base(transform) {}
        
        public override StructureType GetStructureType() {
            return StructureType.Ground;
        }

        public void Encode(BinaryWriter writer) {
            this.Transform.Encode(writer);
            ushort flags = 0;
            writer.Write(flags);
        }

        public static Ground Decode(BinaryReader reader) {
            var transform = Transform.Decode(reader);
            ushort flags = reader.ReadUInt16();
            return new Ground(transform);
        }

        public override string GetEncodingKey() {
            return ENCODING_ID;
        }

        public static Ground Decode(JObject json) {
            var transform = BaseStructure.DecodeTransform(json);
            return new Ground(transform);
        }

        public override IStructureBuilder GetBuilder() {
            return new GroundBuilder(this);
        }

        public class GroundBuilder: BaseStructureBuilder<Ground> {

            protected override string prefabPath => "Prefabs/Structures/Ground";
            protected override CollisionLayer collisionLayer => CollisionLayer.StaticForeground;

            public GroundBuilder(Ground ground): base(ground) {}
        }
    }
}