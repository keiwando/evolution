using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Keiwando.Evolution.Scenes {

    public class Ground: BaseStructure {

        private static readonly string ENCODING_ID = "evolution::structure::ground";

        static Ground() {
            SimulationScene.RegisterStructure(ENCODING_ID, delegate(JObject json) {
                return Decode(json);
            });
        }

        public Ground(Transform transform): base(transform) {}

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

        public class GroundBuilder: BaseStructureBuilder {

            protected override string prefabPath => "Prefabs/Structures/Ground";

            public GroundBuilder(Ground ground): base(ground) {}
        }
    }
}