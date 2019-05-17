using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;
using Keiwando.Evolution.Scenes;

class SceneController {

    public enum Scene {
        Editor,
        SimulationContainer
    }

    public enum SimulationSceneType {
        Simulation,
        BestCreatures
    }

    public class SimulationSceneLoadConfig {

        public readonly CreatureDesign CreatureDesign;
        public readonly int CreatureSpawnCount;
        public readonly SimulationScene SceneDescription;
        public readonly SimulationSceneType SceneType;
        public readonly Vector3 SpawnPoint;

        public SimulationSceneLoadConfig(
            CreatureDesign design,
            int spawnCount,
            SimulationScene sceneDescription,
            SimulationSceneType sceneType,
            Vector3 spawnPoint
        ) {
            this.CreatureDesign = design;
            this.CreatureSpawnCount = spawnCount;
            this.SceneDescription = sceneDescription;
            this.SceneType = sceneType;
            this.SpawnPoint = spawnPoint;
        }
    }

    public class SimulationSceneLoadContext {
        public PhysicsScene PhysicsScene { get; set; }
        public UnityEngine.SceneManagement.Scene Scene { get; set; }
        public Creature[] Creatures { get; set; }
    }

    public static void LoadSync(Scene scene) {

        SceneManager.LoadScene(NameForScene(scene));
    }

    public static IEnumerator LoadSimulationScene(SimulationSceneLoadConfig config, SimulationSceneLoadContext context) {
        
        // Load Scene
        var sceneName = NameForScene(config.SceneType);
        var options = new LoadSceneParameters(LoadSceneMode.Additive, LocalPhysicsMode.Physics3D);
        SceneManager.LoadScene(sceneName, options);
        var scene = SceneManager.GetSceneByName(sceneName);
        context.PhysicsScene = scene.GetPhysicsScene();
        // We need to wait two frames before the scene and its GameObjects
        // are fully loaded
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        // Find setup script in the new scene
        var prevActiveScene = SceneManager.GetActiveScene();
        SceneManager.SetActiveScene(scene);
        var sceneSetup = GameObject.FindObjectOfType<SimulationSceneSetup>();

        // Create the structures
        sceneSetup.BuildScene(config.SceneDescription);
        yield return new WaitForEndOfFrame();

        // Spawn Creatures
        var creatures = sceneSetup.SpawnBatch(config.CreatureDesign, config.CreatureSpawnCount, config.SpawnPoint);
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
        default: 
            throw new System.ArgumentException("Unhandled scene type!");
        }
    }
}