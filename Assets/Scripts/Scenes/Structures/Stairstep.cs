using Keiwando.JSON;
using UnityEngine;

namespace Keiwando.Evolution.Scenes {

    public class Stairstep: BaseStructure {

        public const string ENCODING_ID = "evolution::structure::stairstep";

        public Stairstep(Transform transform): base(transform) {}

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