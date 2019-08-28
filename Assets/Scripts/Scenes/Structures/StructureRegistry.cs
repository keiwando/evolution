namespace Keiwando.Evolution.Scenes {

    public struct RegisteredStructure {
        public string id;
        public SimulationSceneDescription.DecodeStructure decoder;
        public RegisteredStructure(string id, SimulationSceneDescription.DecodeStructure decoder) {
            this.id = id; this.decoder = decoder;
        }
    }

    public static class StructureRegistry {

        public static readonly RegisteredStructure[] Structures = new [] {
            new RegisteredStructure(Ground.ENCODING_ID, Ground.Decode),
            new RegisteredStructure(DistanceMarkerSpawner.ENCODING_ID, DistanceMarkerSpawner.Decode),
            new RegisteredStructure(RollingObstacleSpawner.ENCODING_ID, RollingObstacleSpawner.Decode),
            new RegisteredStructure(Stairstep.ENCODING_ID, Stairstep.Decode),
            new RegisteredStructure(Wall.ENCODING_ID, Wall.Decode),
        };
    }
}