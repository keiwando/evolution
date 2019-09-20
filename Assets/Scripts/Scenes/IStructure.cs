using Keiwando.JSON;

namespace Keiwando.Evolution.Scenes {

    public interface IStructure {

        string GetEncodingKey();
        JObject Encode();
        IStructureBuilder GetBuilder();
    }
}