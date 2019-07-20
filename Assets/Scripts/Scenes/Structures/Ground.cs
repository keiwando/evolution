using Keiwando.JSON;

using UnityEngine;

namespace Keiwando.Evolution.Scenes {

    [RegisterInScene(ENCODING_ID)]
    public class Ground: BaseStructure {

        private const string ENCODING_ID = "evolution::structure::ground";

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

        public class GroundBuilder: BaseStructureBuilder<Ground> {

            protected override string prefabPath => "Prefabs/Structures/Ground";

            public GroundBuilder(Ground ground): base(ground) {}
        }
    }
}