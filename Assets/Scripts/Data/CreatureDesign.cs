using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

    public static readonly CreatureDesign Empty = new CreatureDesign(
        "", new List<JointData>(), new List<BoneData>(), new List<MuscleData>()
    );

    public CreatureDesign(string name, List<JointData> joints, 
                          List<BoneData> bones, List<MuscleData> muscles) {
        this.Name = name;
        this.Joints = joints;
        this.Bones = bones;
        this.Muscles = muscles;
    }

    public CreatureDesign() : 
        this("Unnamed", new List<JointData>(), new List<BoneData>(), new List<MuscleData>()) {}

    #region Encode & Decode

    private static class CodingKey {
        public const string Name = "name";
        public const string Joints = "joints";
        public const string Bones = "bones";
        public const string Muscles = "muscles";
    }

    public JObject Encode() {
        
        var json = new JObject();

        json[CodingKey.Name] = this.Name;
        json[CodingKey.Joints] = JToken.FromObject(this.Joints.Select(joint => joint.Encode()).ToList());
        json[CodingKey.Bones] = JToken.FromObject(this.Bones.Select(bone => bone.Encode()).ToList());
        json[CodingKey.Muscles] = JToken.FromObject(this.Muscles.Select(muscle => muscle.Encode()).ToList());

        return json;
    }

    public static CreatureDesign Decode(string encoded) {

        if (string.IsNullOrEmpty(encoded))
            return Empty;

        JObject json = JObject.Parse(encoded);
        return Decode(json);        
    }

    public static CreatureDesign Decode(JObject json) {
        
        string name = json[CodingKey.Name].ToObject<string>();
        var encodedJoints = json[CodingKey.Joints].ToObject<List<JObject>>();
        var encodedBones = json[CodingKey.Bones].ToObject<List<JObject>>();
        var encodedMuscles = json[CodingKey.Muscles].ToObject<List<JObject>>();

        var joints = encodedJoints.Select(jointJson => JointData.Decode(jointJson)).ToList();
        var bones = encodedBones.Select(boneJson => BoneData.Decode(boneJson)).ToList();
        var muscles = encodedMuscles.Select(muscleJson => MuscleData.Decode(muscleJson)).ToList();

        return new CreatureDesign(name, joints, bones, muscles);
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
        stringBuilder.AppendLine("----------------------");
        return stringBuilder.ToString();
    }

    #endregion
}