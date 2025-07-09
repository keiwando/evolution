using System.IO;
using Keiwando.JSON;
using UnityEngine;

namespace Keiwando.Evolution.Scenes {

    public class Stairstep: BaseStructure {

        public const string ENCODING_ID = "evolution::structure::stairstep";

        public Stairstep(Transform transform): base(transform) {}
        
        public override StructureType GetStructureType() {
            return StructureType.Stairstep;
        }

        public void Encode(BinaryWriter writer) {
            this.Transform.Encode(writer);
            ushort flags = 0; // No flags currently used
            writer.Write(flags);
        }

        public static Stairstep Decode(BinaryReader reader) {
            var transform = Transform.Decode(reader);
            ushort flags = reader.ReadUInt16(); // Read flags, currently unused
            return new Stairstep(transform);
        }

        public override string GetEncodingKey() {
            return ENCODING_ID;
        }

        public static Stairstep Decode(JObject json) {
            var transform = BaseStructure.DecodeTransform(json);
            return new Stairstep(transform);
        }

        public override IStructureBuilder GetBuilder() {
            return new StairstepBuilder(this);
        }

        public class StairstepBuilder: BaseStructureBuilder<Stairstep> {

            protected override string prefabPath => "Prefabs/Structures/Stairstep";
            protected override CollisionLayer collisionLayer => CollisionLayer.StaticForeground;

            public StairstepBuilder(Stairstep step): base(step) {}
        }
    }
}