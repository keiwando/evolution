using UnityEngine.SceneManagement;

class SceneController {

    public enum Scene {
        Editor,
        Simulation
    }

    public static void LoadSync(Scene scene) {

        SceneManager.LoadScene(NameForScene(scene));
    }

    private static string NameForScene(Scene scene) {
        switch (scene) {
            case Scene.Editor: return "EditorScene";
            case Scene.Simulation: return "SimulationScene";
            default: return "EditorScene";
        }
    }
}