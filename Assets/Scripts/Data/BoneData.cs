using System;
using Keiwando.JSON;

[Serializable]
public struct BoneData: IJsonConvertible {

    public readonly int id;
    public readonly int startJointID;
    public readonly int endJointID;
    
    public readonly float weight;
    public readonly bool isWing;
    public readonly bool inverted;

    public readonly bool legacy;

    public BoneData(int id, int startJointID, int endJointID, float weight, bool isWing = false, bool inverted = false, bool legacy = false) {
        this.id = id;
        this.startJointID = startJointID;
        this.endJointID = endJointID;
        this.weight = weight;
        this.isWing = isWing;
        this.inverted = inverted;
        this.legacy = legacy;
    }

    #region Encode & Decode

    private static class CodingKey {
        public const string ID = "id";
        public const string StartJointID = "startJointID";
        public const string EndJointID = "endJointID";
        public const string Weight = "weight";
        public const string Legacy = "legacy";
        // This is optional and only set if true.
        public const string IsWing = "wing";
        // This is optional and only set if true.
        public const string Inverted = "inverted";
    }

    public JObject Encode() {
        var json = new JObject();
        json[CodingKey.ID] = this.id;
        json[CodingKey.StartJointID] = this.startJointID;
        json[CodingKey.EndJointID] = this.endJointID;
        json[CodingKey.Weight] = this.weight;
        json[CodingKey.Legacy] = this.legacy;
        if (this.isWing) {
            json[CodingKey.IsWing] = true;
        }
        if (this.inverted) {
            json[CodingKey.Inverted] = true;
        }
        return json;
    }

    public static BoneData Decode(JObject json) {

        int id = json[CodingKey.ID].ToInt();
        int startID = json[CodingKey.StartJointID].ToInt();
        int endID = json[CodingKey.EndJointID].ToInt();
        float weight = json[CodingKey.Weight].ToFloat();
        bool legacy = json.ContainsKey(CodingKey.Legacy) ? json[CodingKey.Legacy].ToBool() : true;
        bool isWing = json.ContainsKey(CodingKey.IsWing) ? json[CodingKey.IsWing].ToBool() : false;
        bool inverted = json.ContainsKey(CodingKey.Inverted) ? json[CodingKey.Inverted].ToBool() : false;

        return new BoneData(id, startID, endID, weight, isWing, inverted, legacy);
    }

    #endregion
}