using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Keiwando.Evolution.Scenes {

    public class Wall: BaseStructure {

        private static readonly string ENCODING_ID = "evolution::structure::wall";

        static Wall() {
            SimulationScene.RegisterStructure(ENCODING_ID, delegate(JObject json) {
                return Decode(json);
            });
        }

        public Wall(Transform transform): base(transform) {}

        public override string GetEncodingKey() {
            return ENCODING_ID;
        }

        public static Wall Decode(JObject json) {
            var transform = BaseStructure.DecodeTransform(json);
            return new Wall(transform);
        }

        public override IStructureBuilder GetBuilder() {
            return new WallBuilder(this);
        }

        public class WallBuilder: BaseStructureBuilder<Wall> {

            protected override string prefabPath => "Prefabs/Structures/Wall";

            public WallBuilder(Wall wall): base(wall) {}
        }
    }
}