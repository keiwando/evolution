using System.IO;
using Keiwando.JSON;
using UnityEngine;

namespace Keiwando.Evolution.Scenes {

    public class DistanceMarkerSpawner: BaseStructure {

        public const string ENCODING_ID = "evolution::structure::distancemarkerspawner";

        public float MarkerDistance { get; private set; }
        public float DistanceAngleFactor { get; private set; }
        public float BestMarkerRotation { get; private set; }

        public DistanceMarkerSpawner(
            Transform transform, 
            float markerDistance = 5f, 
            float angleFactor = 1f,
            float bestMarkerRotation = 0f
        ): base(transform) {
            this.MarkerDistance = markerDistance;
            this.DistanceAngleFactor = angleFactor;
            this.BestMarkerRotation = bestMarkerRotation;
        }

        public override StructureType GetStructureType() {
            return StructureType.DistanceMarkerSpawner;
        }

        public void Encode(BinaryWriter writer) {
            this.Transform.Encode(writer);
            ushort flags = 0;
            writer.Write(flags);
            writer.Write(MarkerDistance);
            writer.Write(DistanceAngleFactor);
            writer.Write(BestMarkerRotation);
        }

        public static DistanceMarkerSpawner Decode(BinaryReader reader) {
            var transform = Transform.Decode(reader);
            ushort flags = reader.ReadUInt16();
            var markerDistance = reader.ReadSingle();
            var angleFactor = reader.ReadSingle();
            var bestMarkerRotation = reader.ReadSingle();
            return new DistanceMarkerSpawner(
                transform: transform, 
                markerDistance: markerDistance, 
                angleFactor: angleFactor, 
                bestMarkerRotation: bestMarkerRotation
            );
        }

        public override string GetEncodingKey() {
            return ENCODING_ID;
        }

        private static class CodingKey {
            public const string MarkerDistance = "markerDistance";
            public const string AngleFactor = "distanceAngleFactor";
            public const string BestMarkerRotation = "bestMarkerRotation";
        }

        public override JObject Encode() {
            var json = base.Encode();
            json[CodingKey.MarkerDistance] = MarkerDistance;
            json[CodingKey.AngleFactor] = DistanceAngleFactor;
            json[CodingKey.BestMarkerRotation] = BestMarkerRotation;
            return json;
        }

        public static DistanceMarkerSpawner Decode(JObject json) {
            var transform = BaseStructure.DecodeTransform(json);
            var markerDistance = json[CodingKey.MarkerDistance].ToFloat();
            var angleFactor = json[CodingKey.AngleFactor].ToFloat();
            var bestMarkerRotation = json[CodingKey.BestMarkerRotation].ToFloat();
            return new DistanceMarkerSpawner(transform, markerDistance, angleFactor, bestMarkerRotation);
        }

        public override IStructureBuilder GetBuilder() {
            return new DistanceMarkerSpawnerBuilder(this);
        }

        public class DistanceMarkerSpawnerBuilder: BaseStructureBuilder<DistanceMarkerSpawner> {

            protected override string prefabPath => "Prefabs/Structures/DistanceMarkerSpawner";
            protected override CollisionLayer collisionLayer => CollisionLayer.Background;

            public DistanceMarkerSpawnerBuilder(DistanceMarkerSpawner spawner): base(spawner) {}

            public override GameObject Build(ISceneContext context) {

                var spawner = base.Build(context).GetComponent<DistanceMarkerSpawnerBehaviour>();
                spawner.MarkerDistance = this.structure.MarkerDistance;
                spawner.DistanceAngleFactor = this.structure.DistanceAngleFactor;
                spawner.BestMarkerRotation = this.structure.BestMarkerRotation;
                spawner.Context = context;
                return spawner.gameObject;
            }
        }
    }
}