using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Keiwando.JSON;

public class CreatureDesign: IJsonConvertible {

    public readonly int Version = 2;

    public bool IsEmpty {
        get { return Joints.Count == 0; }
    }

    public string Name;
    public readonly List<JointData> Joints;
    public readonly List<BoneData> Bones;
    public readonly List<MuscleData> Muscles;
    public readonly List<DecorationData> Decorations;

    public static readonly CreatureDesign Empty = new CreatureDesign(
        "", new List<JointData>(), new List<BoneData>(), new List<MuscleData>(), new List<DecorationData>()
    );

    public CreatureDesign(string name, 
                          List<JointData> joints, List<BoneData> bones, 
                          List<MuscleData> muscles, List<DecorationData> decorations) {
        this.Name = name;
        this.Joints = joints;
        this.Bones = bones;
        this.Muscles = muscles;
        this.Decorations = decorations;
    }

    public CreatureDesign() : 
        this("Unnamed", new List<JointData>(), new List<BoneData>(), new List<MuscleData>(), new List<DecorationData>()) {}

    #region Encode & Decode

    private static class CodingKey {
        public const string Name = "name";
        public const string Joints = "joints";
        public const string Bones = "bones";
        public const string Muscles = "muscles";
        public const string Decorations = "decorations";
    }

    public JObject Encode() {
        
        var json = new JObject();

        json[CodingKey.Name] = this.Name;
        json[CodingKey.Joints] = JArray.From(this.Joints);
        json[CodingKey.Bones] = JArray.From(this.Bones);
        json[CodingKey.Muscles] = JArray.From(this.Muscles);
        json[CodingKey.Decorations] = JArray.From(this.Decorations);

        return json;
    }

    public static CreatureDesign Decode(string encoded) {

        if (string.IsNullOrEmpty(encoded))
            return Empty;

        JObject json = JObject.Parse(encoded);
        return Decode(json);        
    }

    public static CreatureDesign Decode(JObject json) {
        
        string name = json[CodingKey.Name].ToString();
        var joints = json[CodingKey.Joints].ToList(JointData.Decode);
        var bones = json[CodingKey.Bones].ToList(BoneData.Decode);
        var muscles = json[CodingKey.Muscles].ToList(MuscleData.Decode);
        List<DecorationData> decorations;
        if (json.ContainsKey(CodingKey.Decorations)) {
            decorations = json[CodingKey.Decorations].ToList(DecorationData.Decode);
        } else {
            decorations = new List<DecorationData>();
        }

        return new CreatureDesign(name, joints, bones, muscles, decorations);
    }

    #endregion

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
        foreach (var decoration in Decorations) {
            stringBuilder.AppendLine(string.Format("Decoration {0} - {1} - {2}", decoration.id, decoration.boneId, (int)decoration.decorationType));
        }
        stringBuilder.AppendLine("----------------------");
        return stringBuilder.ToString();
    }

    #endregion
}
