using System.IO;
using Keiwando.JSON;
using UnityEngine;

namespace Keiwando.Evolution.Scenes {

    public class RollingObstacleSpawner: BaseStructure {

        public const string ENCODING_ID = "evolution::structure::rollingobstaclespawner";

        public float SpawnInterval { get; private set; }
        public float ObstacleLifetime { get; private set; }
        public float ForceMultiplier { get; private set; }

        public RollingObstacleSpawner(
            Transform transform,
            float spawnInterval = 5f,
            float obstacleLifetime = 5f,
            float forceMultiplier = 1f
        ): base(transform) {
            this.SpawnInterval = spawnInterval;
            this.ObstacleLifetime = obstacleLifetime;
            this.ForceMultiplier = forceMultiplier;
        } 
        
        public override StructureType GetStructureType() {
            return StructureType.RollingObstacleSpawner;
        }

        public void Encode(BinaryWriter writer) {
            this.Transform.Encode(writer);
            ushort flags = 0; // No flags currently used
            writer.Write(flags);
            writer.Write(SpawnInterval);
            writer.Write(ObstacleLifetime);
            writer.Write(ForceMultiplier);
        }

        public static RollingObstacleSpawner Decode(BinaryReader reader) {
            var transform = Transform.Decode(reader);
            ushort flags = reader.ReadUInt16(); // Read flags, currently unused
            var spawnInterval = reader.ReadSingle();
            var obstacleLifetime = reader.ReadSingle();
            var forceMultiplier = reader.ReadSingle();
            return new RollingObstacleSpawner(
                transform: transform,
                spawnInterval: spawnInterval,
                obstacleLifetime: obstacleLifetime,
                forceMultiplier: forceMultiplier
            );
        }

        public override string GetEncodingKey() {
            return ENCODING_ID;
        }

        private static class CodingKey {
            public const string SpawnInterval = "spawnInterval";
            public const string ObstacleLifetime = "obstacleLifetime";
            public const string ForceMultiplier = "forceMultiplier";
        }

        public override JObject Encode() {
            var json = base.Encode();
            json[CodingKey.SpawnInterval] = this.SpawnInterval;
            json[CodingKey.ObstacleLifetime] = this.ObstacleLifetime;
            json[CodingKey.ForceMultiplier] = this.ForceMultiplier;
            return json;
        }

        public static RollingObstacleSpawner Decode(JObject json) {
            var transform = BaseStructure.DecodeTransform(json);
            var spawnInterval = json[CodingKey.SpawnInterval].ToFloat();
            var obstacleLifetime = json[CodingKey.ObstacleLifetime].ToFloat();
            var forceMultiplier = json[CodingKey.ForceMultiplier].ToFloat();
            return new RollingObstacleSpawner(transform);
        }

        public override IStructureBuilder GetBuilder() {
            return new RollingObstacleSpawnerBuilder(this);
        }

        public class RollingObstacleSpawnerBuilder: IStructureBuilder {

            private RollingObstacleSpawner structure;

            public RollingObstacleSpawnerBuilder(RollingObstacleSpawner structure) {
                this.structure = structure;
            }

            public GameObject Build(ISceneContext context) {

                var spawner = StructureBuilderUtils.AddGameObjectWithBehaviour<RollingObstacleSpawnerBehaviour>(structure.Transform);
                spawner.SpawnInterval = structure.SpawnInterval;
                spawner.ObstacleLifetime = structure.ObstacleLifetime;
                spawner.ForceMultiplier = structure.ForceMultiplier;
                spawner.Context = context;
                return spawner.gameObject;
            }
        }
    }
}