using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Keiwando.Evolution.Scenes {

    public class DistanceMarkerSpawner: BaseStructure {

        private static readonly string ENCODING_ID = "evolution::structure::distancemarkerspawner";

        // TODO: Add more customization options

        static DistanceMarkerSpawner() {
            SimulationScene.RegisterStructure(ENCODING_ID, delegate(JObject json) {
                return Decode(json);
            });
        }

        public DistanceMarkerSpawner(Transform transform): base(transform) {}

        public override string GetEncodingKey() {
            return ENCODING_ID;
        }

        public static DistanceMarkerSpawner Decode(JObject json) {
            var transform = BaseStructure.DecodeTransform(json);
            return new DistanceMarkerSpawner(transform);
        }

        public override IStructureBuilder GetBuilder() {
            return new DistanceMarkerSpawnerBuilder(this);
        }

        public class DistanceMarkerSpawnerBuilder: BaseStructureBuilder {

            protected override string prefabPath => "Prefabs/Structures/DistanceMarkerSpawner";

            public DistanceMarkerSpawnerBuilder(DistanceMarkerSpawner step): base(step) {}
        }
    }
}