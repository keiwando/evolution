using System;

[Serializable]
public struct BoneData {

    public int ID;
    public int StartJointID;
    public int EndJointID;
    
    public float Weight;
}