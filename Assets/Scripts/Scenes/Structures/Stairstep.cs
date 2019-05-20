using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Keiwando.Evolution.Scenes {

    public class Stairstep: BaseStructure {

        private static readonly string ENCODING_ID = "evolution::structure::stairstep";

        static Stairstep() {
            SimulationScene.RegisterStructure(ENCODING_ID, delegate(JObject json) {
                return Decode(json);
            });
        }

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

            public StairstepBuilder(Stairstep step): base(step) {}
        }
    }
}