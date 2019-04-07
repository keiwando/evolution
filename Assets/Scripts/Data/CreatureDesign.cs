using System;

[Serializable]
public class CreatureDesign {

    public readonly int Version = 2;

    public string Name;
    public JointData[] joints;
    public BoneData[] bones;
    public MuscleData[] muscles;
}