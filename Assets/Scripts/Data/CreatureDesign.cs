using System;
using System.Collections.Generic;
using System.Text;

[Serializable]
public class CreatureDesign {

    public readonly int Version = 2;

    public bool IsEmpty {
        get { return Joints.Count == 0; }
    }

    public string Name;
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

    public CreatureDesign() : 
        this("Unnamed", new List<JointData>(), new List<BoneData>(), new List<MuscleData>()) {}

    #region DEBUG

    public string GetDebugDescription() {
        var stringBuilder = new StringBuilder();
        foreach (var joint in Joints) {
            stringBuilder.AppendLine(string.Format("Joint {0} - {1} - {2}", joint.id, joint.position.x, joint.position.y));
        }
        foreach (var bone in Bones) {
            stringBuilder.AppendLine(string.Format("Bone {0} - {1} - {2}", bone.id, bone.startJointID, bone.endJointID));
        }
        foreach (var muscle in Muscles) {
            stringBuilder.AppendLine(string.Format("Muscle {0} - {1} - {2}", muscle.id, muscle.startBoneID, muscle.endBoneID));
        }
        stringBuilder.AppendLine("----------------------");
        return stringBuilder.ToString();
    }

    #endregion
}