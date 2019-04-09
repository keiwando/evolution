using System;
using System.Collections.Generic;

[Serializable]
public class CreatureDesign {

    public readonly int Version = 2;

    public string Name;
    public List<JointData> joints;
    public List<BoneData> bones;
    public List<MuscleData> muscles;
}