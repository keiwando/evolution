using System;

[Serializable]
public struct MuscleData {

    public int ID;
    public int StartBoneID;
    public int EndBoneID;

    public float Strength;
    public bool CanExpand;
}