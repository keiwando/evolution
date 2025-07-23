using System;
using UnityEngine;
using Keiwando.JSON;

public enum DecorationType: int {
  GooglyEye = 0,
  Emoji_Eyes = 1,
  Emoji_Smile = 2,
  Emoji_Neutral_Face = 3,
  Emoji_No_Mouth = 4,
  Emoji_Grimacing = 5,
  Emoji_Sunglasses_Face = 6,
  Emoji_Skull = 7,
  Emoji_Clown = 8,
  Emoji_Alien = 9,
  Emoji_Robot = 10,
  Emoji_Waving = 11,
  Emoji_Hand = 12,
  Emoji_Peace = 13,
  Emoji_Horns = 14,
  Emoji_Call_Me = 15,
  Emoji_Leg = 16,
  Emoji_Foot = 17,
  Emoji_Nose = 18,
  Emoji_Brain = 19,
  Emoji_Eye = 20,
  Emoji_Mouth = 21,
  Emoji_Dog = 22,
  Emoji_Cat = 23,
  Emoji_Unicorn = 24,
  Emoji_Wheel = 25,
  Emoji_Shoe1 = 26,
  Emoji_Shoe2 = 27,
  Emoji_Shoe3 = 28,
  Emoji_Shoe4 = 29,
  Emoji_Shoe5 = 30,
  Emoji_Shoe6 = 31,
  Emoji_Shoe7 = 32,
  Emoji_Hat = 33,
  Emoji_Crown = 34,
  Emoji_Top_Hat = 35,
  Emoji_Saxophone = 36,
  Emoji_Guitar = 37,
  Emoji_Bone = 38
}

[Serializable]
public struct DecorationData: IJsonConvertible {

  public readonly int id;
  public readonly int boneId;
  
  public Vector2 offset;
  public float scale;
  public float rotation;
  public readonly bool flipX;
  public readonly bool flipY;
  public readonly DecorationType decorationType;

  public DecorationData(
    int id,
    int boneId,
    Vector3 offset,
    float scale,
    float rotation,
    bool flipX,
    bool flipY,
    DecorationType decorationType
  ) {
    this.id = id;
    this.boneId = boneId;
    this.offset = offset;
    this.scale = scale;
    this.rotation = rotation;
    this.flipX = flipX;
    this.flipY = flipY;
    this.decorationType = decorationType;
  }

  #region Encode & Decode

  private static class CodingKey {
    public const string ID = "id";
    public const string BoneID = "boneID";
    public const string OffsetX = "x";
    public const string OffsetY = "y";
    public const string Scale = "s";
    public const string Rotation = "r";
    public const string flipX = "fX";
    public const string flipY = "fY";
    public const string decorationType = "dT"; 
  }

  public JObject Encode() {
    var json = new JObject();
    json[CodingKey.ID] = this.id;
    json[CodingKey.BoneID] = this.boneId;
    json[CodingKey.OffsetX] = this.offset.x;
    json[CodingKey.OffsetY] = this.offset.y;
    json[CodingKey.Scale] = this.scale;
    json[CodingKey.Rotation] = this.rotation;
    json[CodingKey.flipX] = this.flipX;
    json[CodingKey.flipY] = this.flipY;
    json[CodingKey.decorationType] = (int)this.decorationType;
    return json;
  }

  public static DecorationData Decode(JObject json) {
    int id = json[CodingKey.ID].ToInt();
    int boneId = json[CodingKey.BoneID].ToInt();
    float offsetX = json[CodingKey.OffsetX].ToFloat();
    float offsetY = json[CodingKey.OffsetY].ToFloat();
    float scale = json[CodingKey.Scale].ToFloat();
    float rotation = json[CodingKey.Rotation].ToFloat();
    bool flipX = json[CodingKey.flipX].ToBool();
    bool flipY = json[CodingKey.flipY].ToBool();
    DecorationType decorationType = (DecorationType)json[CodingKey.decorationType].ToInt();

    return new DecorationData(id, boneId, new Vector2(offsetX, offsetY), scale, rotation, flipX, flipY, decorationType);
  }

  #endregion
}
