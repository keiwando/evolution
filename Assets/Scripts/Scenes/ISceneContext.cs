using System;
using UnityEngine;

namespace Keiwando.Evolution.Scenes {

    public interface ISceneContext {

        CreatureStats GetStatsForBestOfGeneration(int generation);
        float GetDistanceOfBest();
        // float GetDistanceOfBest(int generation);
        // int GetCurrentGeneration();

        LayerMask GetBackgroundLayer();
        LayerMask GetStaticForegroundLayer();
        LayerMask GetDynamicForegroundLayer();
    }
}