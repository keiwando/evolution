using System;
using UnityEngine;
using Keiwando.JSON;

[Serializable]
public struct JointData: IJsonConvertible {

    public readonly int id;

    public readonly Vector2 position;
    public readonly float weight;

    public JointData(int id, Vector2 position, float weight) {
        this.id = id;
        this.position = position;
        this.weight = weight;
    }

    #region Encode & Decode

    private static class CodingKey {
        public const string ID = "id";
        public const string X = "x";
        public const string Y = "y";
        public const string Weight = "weight";
    }

    public JObject Encode() {
        var json = new JObject();
        json[CodingKey.ID] = this.id;
        json[CodingKey.X] = this.position.x;
        json[CodingKey.Y] = this.position.y;
        json[CodingKey.Weight] = this.weight;
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

        return new JointData(id, new Vector2(x, y), weight);
    }

    #endregion
}