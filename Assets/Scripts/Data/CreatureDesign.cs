using System;
using System.Collections.Generic;

[Serializable]
public class CreatureDesign {

    public readonly int Version = 2;

    public string Name { get; set; }
    public readonly List<JointData> Joints;
    public readonly List<BoneData> Bones;
    public readonly List<MuscleData> Muscles;

    public CreatureDesign(string name, List<JointData> joints, 
                          List<BoneData> bones, List<MuscleData> muscles) {
        this.Name = name;
        this.Joints = joints;
        this.Bones = bones;
        this.Muscles = muscles;
    }
}