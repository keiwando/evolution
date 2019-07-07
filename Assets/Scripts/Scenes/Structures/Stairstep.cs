using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Keiwando.Evolution.Scenes {

    [RegisterInScene(ENCODING_ID)]
    public class Stairstep: BaseStructure {

        private const string ENCODING_ID = "evolution::structure::stairstep";

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