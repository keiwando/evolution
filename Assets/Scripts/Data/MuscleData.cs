using System;
using Keiwando.JSON;

[Serializable]
public struct MuscleData: IJsonConvertible {

    public readonly int id;
    public readonly int startBoneID;
    public readonly int endBoneID;

    public readonly float strength;
    public readonly bool canExpand;
    public readonly string userId;

    public MuscleData(int id, int startBoneID, int endBoneID, float strength, bool canExpand, string userId) {
        this.id = id;
        this.startBoneID = startBoneID;
        this.endBoneID = endBoneID;
        this.strength = strength;
        this.canExpand = canExpand;
        this.userId = userId;
    }

    #region Encode & Decode

    private static class CodingKey {
        public const string ID = "id";
        public const string StartBoneID = "startBoneID";
        public const string EndBoneID = "endBoneID";
        public const string Strength = "strength";
        public const string CanExpand = "canExpand";
        // This is optional and only serialized if not ""
        public const string UserID = "userID";
    }

    public JObject Encode() {
        var json = new JObject();
        json[CodingKey.ID] = this.id;
        json[CodingKey.StartBoneID] = this.startBoneID;
        json[CodingKey.EndBoneID] = this.endBoneID;
        json[CodingKey.Strength] = this.strength;
        json[CodingKey.CanExpand] = this.canExpand;
        if (string.IsNullOrEmpty(userId)) {
            json[CodingKey.UserID] = userId;
        }
        return json;
    }

    public static MuscleData Decode(string encoded) {

        var json = JObject.Parse(encoded);
        return Decode(json);
    }

    public static MuscleData Decode(JObject json) {

        int id = json[CodingKey.ID].ToInt();
        int startID = json[CodingKey.StartBoneID].ToInt();
        int endID = json[CodingKey.EndBoneID].ToInt();
        float strength = json[CodingKey.Strength].ToInt();
        bool canExpand = json[CodingKey.CanExpand].ToBool();
        string userId = json.ContainsKey(CodingKey.UserID) ? json[CodingKey.UserID].ToString() : "";

        return new MuscleData(id, startID, endID, strength, canExpand, userId);
    }

    #endregion
}