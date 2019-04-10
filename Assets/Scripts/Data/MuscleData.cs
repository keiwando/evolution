using System;

[Serializable]
public struct MuscleData {

    public readonly int id;
    public readonly int startBoneID;
    public readonly int endBoneID;

    public readonly float strength;
    public readonly bool canExpand;

    public MuscleData(int id, int startBoneID, int endBoneID, float strength, bool canExpand) {
        this.id = id;
        this.startBoneID = startBoneID;
        this.endBoneID = endBoneID;
        this.strength = strength;
        this.canExpand = canExpand;
    }
}