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

    [SerializeField]
    private CreatureBuilder creatureBuilder;

    public Tool ActiveTool = Tool.Joint;

    void Start() {
        
    }
}