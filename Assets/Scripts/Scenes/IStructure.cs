using System.IO;
using Keiwando.JSON;

namespace Keiwando.Evolution.Scenes {

    public enum StructureType: ushort {
        Ground = 0,
        Wall = 1,
        DistanceMarkerSpawner = 2,
        RollingObstacleSpawner = 3,
        Stairstep = 4
    }

    public interface IStructure {
        StructureType GetStructureType();

        string GetEncodingKey();
        JObject Encode();
        IStructureBuilder GetBuilder();
    }
}