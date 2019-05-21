using System;

namespace Keiwando.Evolution.Scenes {

    public interface ISceneContext {

        CreatureStats GetStatsForBestOfGeneration(int generation);
        float GetDistanceOfBest(int generation);
        int GetCurrentGeneration();
    }
}