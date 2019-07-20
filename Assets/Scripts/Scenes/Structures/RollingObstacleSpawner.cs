using Keiwando.JSON;

using UnityEngine;

namespace Keiwando.Evolution.Scenes {

    [RegisterInScene(ENCODING_ID)]
    public class RollingObstacleSpawner: BaseStructure {

        private const string ENCODING_ID = "evolution::structure::rollingobstaclespawner";

        public RollingObstacleSpawner(Transform transform): base(transform) {} 

        public override string GetEncodingKey() {
            return ENCODING_ID;
        }

        public static RollingObstacleSpawner Decode(JObject json) {
            var transform = BaseStructure.DecodeTransform(json);
            return new RollingObstacleSpawner(transform);
        }

        public override IStructureBuilder GetBuilder() {
            return new RollingObstacleSpawnerBuilder(this);
        }

        public class RollingObstacleSpawnerBuilder: BaseStructureBuilder<RollingObstacleSpawner> {

            protected override string prefabPath => "Prefabs/Structures/RollingObstacleSpawner";

            public RollingObstacleSpawnerBuilder(RollingObstacleSpawner spawner): base(spawner) {}
        }
    }
}