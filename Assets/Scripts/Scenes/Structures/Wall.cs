using Keiwando.JSON;

using UnityEngine;

namespace Keiwando.Evolution.Scenes {

    public class Wall: BaseStructure {

        public const string ENCODING_ID = "evolution::structure::wall";

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
            protected override CollisionLayer collisionLayer => CollisionLayer.Wall;

            public WallBuilder(Wall wall): base(wall) {}
        }
    }
}