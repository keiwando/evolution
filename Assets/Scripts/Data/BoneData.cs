using System;

[Serializable]
public struct BoneData {

    public readonly int id;
    public readonly int startJointID;
    public readonly int endJointID;
    
    public readonly float weight;

    public BoneData(int id, int startJointID, int endJointID, float weight) {
        this.id = id;
        this.startJointID = startJointID;
        this.endJointID = endJointID;
        this.weight = weight;
    }
}