using UnityEngine;

namespace Keiwando.Evolution.Scenes {

    public interface IStructureBuilder {
        GameObject Build(ISceneContext context);
    }
}