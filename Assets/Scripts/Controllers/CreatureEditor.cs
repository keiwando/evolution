using UnityEngine;

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