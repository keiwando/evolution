using UnityEngine.SceneManagement;

class SceneController {

    public enum Scene {
        Editor,
        SimulationContainer,
        Simulation
    }

    public static void LoadSync(Scene scene) {

        SceneManager.LoadScene(NameForScene(scene));
    }

    public static UnityEngine.SceneManagement.Scene LoadSimulationScene() {

        var options = new LoadSceneParameters(LoadSceneMode.Additive, LocalPhysicsMode.Physics3D);
        SceneManager.LoadScene(NameForScene(Scene.Simulation), options);
        return SceneManager.GetSceneByName(NameForScene(Scene.Simulation));
    }

    private static string NameForScene(Scene scene) {
        switch (scene) {
            case Scene.Editor: return "EditorScene";
            case Scene.SimulationContainer: return "SimulationContainerScene";
            case Scene.Simulation: return "SimulationScene";
            default: return "EditorScene";
        }
    }
}