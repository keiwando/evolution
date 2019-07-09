using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;
using Keiwando;
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

        public SimulationSceneLoadConfig(
            CreatureDesign design,
            int spawnCount,
            SimulationScene sceneDescription,
            SimulationSceneType sceneType
        ) {
            this.CreatureDesign = design;
            this.CreatureSpawnCount = spawnCount;
            this.SceneDescription = sceneDescription;
            this.SceneType = sceneType;
        }
    }

    public class SimulationSceneLoadContext {
        public PhysicsScene PhysicsScene { get; set; }
        public UnityEngine.SceneManagement.Scene Scene { get; set; }
        public Creature[] Creatures { get; set; }
    }

    public static void LoadSync(Scene scene) {

        InputRegistry.shared.DeregisterAll();
        SceneManager.LoadScene(NameForScene(scene));
    }

    public static IEnumerator UnloadAsync(UnityEngine.SceneManagement.Scene scene) {
        yield return SceneManager.UnloadSceneAsync(scene);
    }

    public static IEnumerator LoadSimulationScene(SimulationSceneLoadConfig config, SimulationSceneLoadContext context, ISceneContext sceneContext = null) {
        
        // Load Scene
        var sceneName = NameForScene(config.SceneType);
        var options = new LoadSceneParameters(LoadSceneMode.Additive, LocalPhysicsMode.Physics3D);
    
        SceneManager.LoadScene(sceneName, options);
        var scene = SceneManager.GetSceneByName(sceneName);
        context.PhysicsScene = scene.GetPhysicsScene();
        context.Scene = scene;
        // We need to wait two frames before the scene and its GameObjects
        // are fully loaded
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

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
        sceneSetup.BuildScene(config.SceneDescription, sceneContext);

        SceneManager.SetActiveScene(prevActiveScene);
        yield return new WaitForFixedUpdate();
        SceneManager.SetActiveScene(scene);

        // Spawn Creatures
        var creatures = sceneSetup.SpawnBatch(config.CreatureDesign, config.CreatureSpawnCount, context.PhysicsScene);
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