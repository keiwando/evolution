using System;
using UnityEngine;
using Keiwando.JSON;

[Serializable]
public struct JointData: IJsonConvertible {

    public readonly int id;

    public readonly Vector2 position;
    public readonly float weight;
    public readonly float fitnessPenaltyForTouchingGround;
    public readonly bool isGooglyEye;

    public JointData(int id, Vector2 position, float weight, float penalty, bool isGooglyEye) {
        this.id = id;
        this.position = position;
        this.weight = weight;
        this.fitnessPenaltyForTouchingGround = penalty;
        this.isGooglyEye = isGooglyEye;
    }

    #region Encode & Decode

    private static class CodingKey {
        public const string ID = "id";
        public const string X = "x";
        public const string Y = "y";
        public const string Weight = "weight";
        // This is optional and only serialized if != 0.0f
        public const string Penalty = "penalty";
        public const string IsGooglyEye = "gE";
    }

    public JObject Encode() {
        var json = new JObject();
        json[CodingKey.ID] = this.id;
        json[CodingKey.X] = this.position.x;
        json[CodingKey.Y] = this.position.y;
        json[CodingKey.Weight] = this.weight;
        if (fitnessPenaltyForTouchingGround != 0.0f) {
            json[CodingKey.Penalty] = fitnessPenaltyForTouchingGround;
        }
        if (isGooglyEye) {
            json[CodingKey.IsGooglyEye] = true;
        }
        return json;
    }

    public static JointData Decode(string encoded) {

        var json = JObject.Parse(encoded);
        return Decode(json);
    }

    public static JointData Decode(JObject json) {

        int id = json[CodingKey.ID].ToInt();
        float x = json[CodingKey.X].ToFloat();
        float y = json[CodingKey.Y].ToFloat();
        float weight = json[CodingKey.Weight].ToFloat();
        float penalty = json.ContainsKey(CodingKey.Penalty) ? json[CodingKey.Penalty].ToFloat() : 0.0f;
        bool isGooglyEye = json.ContainsKey(CodingKey.IsGooglyEye) && json[CodingKey.IsGooglyEye].ToBool();

        return new JointData(id, new Vector2(x, y), weight, penalty, isGooglyEye);
    }

    #endregion
}