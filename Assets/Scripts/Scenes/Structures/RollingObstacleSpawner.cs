using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Keiwando.Evolution.Scenes {

    public class RollingObstacleSpawner: BaseStructure {

        private static readonly string ENCODING_ID = "evolution::structure::rollingobstaclespawner";

        static RollingObstacleSpawner() {
            SimulationScene.RegisterStructure(ENCODING_ID, delegate(JObject json) {
                return Decode(json);
            });
        }

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

        public class RollingObstacleSpawnerBuilder: BaseStructureBuilder {

            protected override string prefabPath => "Prefabs/Structures/RollingObstacleSpawner";

            public RollingObstacleSpawnerBuilder(RollingObstacleSpawner spawner): base(spawner) {}
        }
    }
}