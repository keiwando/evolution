using UnityEngine;

enum EditorMode {
    Basic = 0,
    Advanced = 1
}

public class CreatureEditor: MonoBehaviour {

    public enum Tool {
        Joint,
        Bone,
        Muscle,
        Select,
        Move,
        Delete
    }

    public Tool ActiveTool = Tool.Joint;

    [SerializeField]
    private CreatureBuilder creatureBuilder;
    [SerializeField]
    private EditorViewController viewController;
    [SerializeField]
    private 

    void Start() {
        
    }

    void Update() {

    }

    /// <summary>
    /// Removes all currently placed body components from the scene
    /// </summary>
    public void Clear() {
        
    }

    

    /// <summary>
    /// Prepares the simulation state and loads the simulation scene
    /// </summary>
    public void StartSimulation () {

        // TODO: Implement
        // Get creature design
        // Get simulation settings
        // Load simulation scene
    }
}