using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;
using Keiwando;
using Keiwando.Evolution.Scenes;
using UnityEngine.EventSystems;

namespace Keiwando.Evolution {

  public class SceneController {

    public enum Scene {
      Editor,
      SimulationContainer
    }

    public enum SimulationSceneType {
      Simulation,
      BestCreatures,
      GalleryPlayback
    }

    public class SimulationSceneLoadContext {
      public PhysicsScene PhysicsScene { get; set; }
      public UnityEngine.SceneManagement.Scene Scene { get; set; }
      public Creature[] Creatures { get; set; }
      public bool DisableAllRenderers = false;
    }

    public static void LoadSync(Scene scene) {

      InputRegistry.shared.DeregisterAll();
      SceneManager.LoadScene(NameForScene(scene));
    }

    public static IEnumerator UnloadAsync(UnityEngine.SceneManagement.Scene scene) {
      yield return SceneManager.UnloadSceneAsync(scene);
    }

    public static IEnumerator LoadSimulationScene(
        CreatureDesign creatureDesign,
        int creatureSpawnCount,
        SimulationSceneDescription sceneDescription,
        SimulationSceneType sceneType,
        LegacySimulationOptions legacyOptions,
        SimulationSceneLoadContext context,
        ISceneContext sceneContext
    ) {

      // Load Scene
      var sceneName = NameForScene(sceneType);
      var options = new LoadSceneParameters(LoadSceneMode.Additive, LocalPhysicsMode.Physics3D);

      var scene = SceneManager.LoadScene(sceneName, options);
      // var scene = SceneManager.GetSceneByName(sceneName);
      context.PhysicsScene = scene.GetPhysicsScene();
      context.Scene = scene;
      // We need to wait before the scene and its GameObjects
      // are fully loaded
      while (!scene.isLoaded) {
        yield return new WaitForEndOfFrame();
      }
      // Find setup script in the new scene
      var prevActiveScene = SceneManager.GetActiveScene();
      SceneManager.SetActiveScene(scene);

      SimulationSceneSetup sceneSetup = null;
      var rootObjects = scene.GetRootGameObjects();
      for (int i = 0; i < rootObjects.Length; i++) {
        sceneSetup = rootObjects[i].GetComponent<SimulationSceneSetup>();
        if (sceneSetup != null) break;
      }

      // Create the structures
      UnityEngine.SceneManagement.Scene? trackedCameraFilterScene = sceneType == SimulationSceneType.GalleryPlayback ? scene : null;
      sceneSetup.BuildScene(sceneDescription, sceneContext, trackedCameraFilterScene: trackedCameraFilterScene);

      SceneManager.SetActiveScene(prevActiveScene);
      yield return new WaitForFixedUpdate();
      SceneManager.SetActiveScene(scene);

      // Spawn Creatures
      var spawnOptions = new SimulationSpawnConfig(
          creatureDesign,
          creatureSpawnCount,
          context.PhysicsScene,
          sceneContext,
          legacyOptions,
          sceneDescription
      );
      var creatures = sceneSetup.SpawnBatch(spawnOptions);
      SceneManager.SetActiveScene(prevActiveScene);
      context.Creatures = creatures;
    }

    private static string NameForScene(Scene scene) {
      switch (scene) {
        case Scene.Editor: return "EditorScene";
        case Scene.SimulationContainer: return "SimulationContainerScene";
        default:
          throw new System.ArgumentException("Unhandled scene type!");
      }
    }

    private static string NameForScene(SimulationSceneType scene) {
      switch (scene) {
        case SimulationSceneType.Simulation: return "SimulationScene";
        case SimulationSceneType.BestCreatures: return "BestCreaturesScene";
        case SimulationSceneType.GalleryPlayback: return "GalleryPlaybackScene";
        default:
          throw new System.ArgumentException("Unhandled scene type!");
      }
    }
  }
}

