using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

    #region Encode & Decode

    private static class CodingKey {
        public const string ID = "id";
        public const string StartJointID = "startJointID";
        public const string EndJointID = "endJointID";
        public const string Weight = "weight";
    }

    public JObject Encode() {
        var json = new JObject();
        json[CodingKey.ID] = this.id;
        json[CodingKey.StartJointID] = this.startJointID;
        json[CodingKey.EndJointID] = this.endJointID;
        json[CodingKey.Weight] = this.weight;
        return json;
    }

    public static BoneData Decode(string encoded) {

        var json = JObject.Parse(encoded);
        return Decode(json);        
    }

    public static BoneData Decode(JObject json) {

        int id = json[CodingKey.ID].ToObject<int>();
        int startID = json[CodingKey.StartJointID].ToObject<int>();
        int endID = json[CodingKey.EndJointID].ToObject<int>();
        float weight = json[CodingKey.Weight].ToObject<float>();

        return new BoneData(id, startID, endID, weight);
    }

    #endregion
}