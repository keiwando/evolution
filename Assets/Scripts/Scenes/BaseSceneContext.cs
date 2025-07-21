using System.IO;
using UnityEngine;

namespace Keiwando.Evolution.Scenes {

  public abstract class BaseSceneContext {

    protected readonly SimulationData data;

    protected abstract string backgroundLayerName { get; }
    protected abstract string staticForegroundLayerName { get; }
    protected abstract string dynamicForegroundLayerName { get; }

    private readonly LayerMask backgroundLayer;
    private readonly LayerMask staticForegroundLayer;
    private readonly LayerMask dynamicForegroundLayer;

    private Evolution evolution;

    private float cachedBestDistance = 0;
    private int lastGenerationIncludedInCachedBestDistance = -1;

    public BaseSceneContext(Evolution evolution) {
      this.evolution = evolution;
      this.data = evolution.SimulationData;
      this.backgroundLayer = LayerMask.NameToLayer(backgroundLayerName);
      this.staticForegroundLayer = LayerMask.NameToLayer(staticForegroundLayerName);
      this.dynamicForegroundLayer = LayerMask.NameToLayer(dynamicForegroundLayerName);
    }

    public CreatureStats GetStatsForBestOfGeneration(int generation) {
      if (generation < 1 || generation > data.BestCreatures.Count) {
        return null;
      }
      evolution.LoadBestCreatureOfGenerationIfNecessary(generation);
      return data.BestCreatures[generation - 1]?.Stats;
    }

    public virtual float GetDistanceOfBest() {

      if (data.BestCreatures.Count == 0) return float.NaN;

      float bestDistance = cachedBestDistance;

      if (data.BestCreatures.Count > lastGenerationIncludedInCachedBestDistance + 1) {

        BinaryReader reader = null;
        string saveFilePath = evolution.CurrentSaveFilePath;
        int chromosomeLength = 0;
        Objective objective = this.data.Settings.Objective;

        try {
          for (int i = lastGenerationIncludedInCachedBestDistance + 1; i < data.BestCreatures.Count; i++) {
            float distance = 0f;
            if (data.BestCreatures[i] != null) {
              var stats = data.BestCreatures[i].Value.Stats;
              distance = GetDistanceForObjective(stats, objective);

            } else if (!string.IsNullOrEmpty(saveFilePath) && File.Exists(saveFilePath)) {
              if (reader == null) {
                var stream = File.Open(saveFilePath, FileMode.Open);
                reader = new BinaryReader(stream, System.Text.Encoding.UTF8);
                chromosomeLength = SimulationSerializer.SkipUntilBestCreaturesDataAndReturnChromosomeLength(reader);
                int numberOfBestCreatureEntries = reader.ReadInt32();
                SimulationSerializer.SkipBestCreatureEntries(reader, count: i, chromosomeLength);
              }
              reader.BaseStream.Seek(chromosomeLength * sizeof(float), SeekOrigin.Current);
              distance = GetDistanceForObjectiveAndAdvancePastCreatureStats(reader, objective);
            }
            
            if (distance > bestDistance) {
              bestDistance = distance;
            }
          }
        } finally {
          if (reader != null) {
            reader.BaseStream.Close();
            reader.Close();
          }
        }
        
        lastGenerationIncludedInCachedBestDistance = data.BestCreatures.Count - 1;
        cachedBestDistance = bestDistance;
      }

      return bestDistance;
    }

    public LayerMask GetBackgroundLayer() {
      return backgroundLayer;
    }

    public LayerMask GetDynamicForegroundLayer() {
      return dynamicForegroundLayer;
    }

    public LayerMask GetStaticForegroundLayer() {
      return staticForegroundLayer;
    }

    public static float GetDistanceForObjective(CreatureStats stats, Objective objective) {
      if (stats == null) return float.NaN;
      switch (objective) {
        case Objective.Running:
          return stats.horizontalDistanceTravelled;
        case Objective.ObstacleJump:
        case Objective.Climbing:
        case Objective.Flying:
          return stats.verticalDistanceTravelled;
        case Objective.Jumping:
          return stats.maxJumpingHeight;
      }
      return stats.horizontalDistanceTravelled;
    }

    // This assumes that the binary reader offset is currently at the beginning of a CreatureStats encoding.
    public static float GetDistanceForObjectiveAndAdvancePastCreatureStats(BinaryReader reader, Objective objective) {
      switch (objective) {
        case Objective.Running:
          return CreatureStats.DecodeHorizontalDistanceTravelledAndAdvanceToEnd(reader);
        case Objective.ObstacleJump:
        case Objective.Climbing:
        case Objective.Flying:
          return CreatureStats.DecodeVerticalDistanceTravelledAndAdvanceToEnd(reader);
        case Objective.Jumping:
          return CreatureStats.DecodeMaxJumpingHeightAndAdvanceToEnd(reader);
        default:
          Debug.LogError("Handle all cases explicitly");
          return CreatureStats.DecodeHorizontalDistanceTravelledAndAdvanceToEnd(reader);
      }
    }
  }
}