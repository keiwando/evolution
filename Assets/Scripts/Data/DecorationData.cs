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
  public bool flipX;
  public bool flipY;
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

public static class DecorationUtils {

  static Sprite[] decorationEmojiSprites;
  static Sprite googlyEyeSprite;

  public static Sprite DecorationTypeToImageResourceName(DecorationType type) {
    switch (type) {
      case DecorationType.GooglyEye: 
        if (googlyEyeSprite == null) {
          googlyEyeSprite = Resources.Load<Sprite>("Sprites/Decorations/googly_eye_icon");
        }
        return googlyEyeSprite;
      default:
        int index = (int)type - 1;
        if (decorationEmojiSprites == null) {
          decorationEmojiSprites = Resources.LoadAll<Sprite>("Sprites/Decorations/decoration_emojis");
        }
        return decorationEmojiSprites[index];
    }
  } 

  public static Vector3 HitboxSizeForDecoration(DecorationType type) {
    switch (type) {
      case DecorationType.GooglyEye: // Note: This one is unused, since it's a manually defined sphere collider.
        return new Vector3(0.55f, 0.55f, 0.55f);
      case DecorationType.Emoji_Eyes:
        return new Vector3(2.0f, 1.45f, 0.2f);
      case DecorationType.Emoji_Smile:
      case DecorationType.Emoji_Neutral_Face:
      case DecorationType.Emoji_No_Mouth:
      case DecorationType.Emoji_Grimacing:
      case DecorationType.Emoji_Sunglasses_Face:
      case DecorationType.Emoji_Clown:
        return new Vector3(1.6f, 1.6f, 0.2f);
      case DecorationType.Emoji_Skull:
      case DecorationType.Emoji_Alien:
        return new Vector3(1.34f, 1.68f, 0.2f);
      case DecorationType.Emoji_Robot:
        return new Vector3(1.61f, 1.56f, 0.2f);
      case DecorationType.Emoji_Waving:
        return new Vector3(1.49f, 1.68f, 0.2f);
      case DecorationType.Emoji_Hand:
        return new Vector3(1.88f, 1.28f, 0.2f);
      case DecorationType.Emoji_Peace:
      case DecorationType.Emoji_Horns:
        return new Vector3(1.19f, 2.16f, 0.2f);
      case DecorationType.Emoji_Call_Me:
        return new Vector3(1.61f, 2.0f, 0.2f);
      case DecorationType.Emoji_Leg:
        return new Vector3(0.86f, 2.08f, 0.2f);
      case DecorationType.Emoji_Foot:
        return new Vector3(1.34f, 1.53f, 0.2f);
      case DecorationType.Emoji_Nose:
        return new Vector3(1.1f, 1.6f, 0.2f);
      case DecorationType.Emoji_Brain:
        return new Vector3(1.78f, 1.32f, 0.2f);
      case DecorationType.Emoji_Eye:
        return new Vector3(2.42f, 1.48f, 0.2f);
      case DecorationType.Emoji_Mouth:
        return new Vector3(1.63f, 1.18f, 0.2f);
      case DecorationType.Emoji_Dog:
      case DecorationType.Emoji_Cat:
        return new Vector3(1.76f, 1.52f, 0.2f);
      case DecorationType.Emoji_Unicorn:
        return new Vector3(1.76f, 1.84f, 0.2f);
      case DecorationType.Emoji_Wheel:
        return new Vector3(1.8f, 1.8f, 0.2f);
      case DecorationType.Emoji_Shoe1:
        return new Vector3(2f, 1f, 0.2f);
      case DecorationType.Emoji_Shoe2:
        return new Vector3(2f, 1f, 0.2f);
      case DecorationType.Emoji_Shoe3:
        return new Vector3(1.85f, 1.16f, 0.2f);
      case DecorationType.Emoji_Shoe4:
        return new Vector3(2f, 0.65f, 0.2f);
      case DecorationType.Emoji_Shoe5:
        return new Vector3(1.55f, 1.52f, 0.2f);
      case DecorationType.Emoji_Shoe6:
        return new Vector3(1.88f, 1.12f, 0.2f);
      case DecorationType.Emoji_Shoe7:
        return new Vector3(1.91f, 0.74f, 0.2f);
      case DecorationType.Emoji_Hat:
        return new Vector3(1.76f, 1.04f, 0.2f);
      case DecorationType.Emoji_Crown:
        return new Vector3(1.58f, 1.36f, 0.2f);
      case DecorationType.Emoji_Top_Hat:
        return new Vector3(1.76f, 1.88f, 0.2f);
      case DecorationType.Emoji_Saxophone:
        return new Vector3(1.01f, 1.88f, 0.2f);
      case DecorationType.Emoji_Guitar:
        return new Vector3(1.64f, 1.52f, 0.2f);
      case DecorationType.Emoji_Bone:
        return new Vector3(2f, 0.76f, 0.2f);
      default:
        return new Vector3(2.0f, 2.0f, 0.2f);
    }
  }

  public static Vector3 HitboxCenterForDecoration(DecorationType type) {
    switch (type) {
      case DecorationType.Emoji_Shoe1:
        return new Vector3(0f, -0.15f, 0f);
      case DecorationType.Emoji_Shoe2:
        return new Vector3(0f, -0.15f, 0f);
      case DecorationType.Emoji_Shoe3:
        return new Vector3(0f, -0.12f, 0f);
      case DecorationType.Emoji_Shoe5:
        return new Vector3(0f, -0.2f, 0f);
      case DecorationType.Emoji_Shoe6:
        return new Vector3(0f, -0.28f, 0f);
      case DecorationType.Emoji_Shoe7:
        return new Vector3(0f, -0.36f, 0f);
      case DecorationType.Emoji_Hat:
        return new Vector3(0f, 0.07f, 0f);
      case DecorationType.Emoji_Top_Hat:
        return new Vector3(0f, 0.15f, 0f);
      case DecorationType.Emoji_Saxophone:
        return new Vector3(0.16f, -0.06f, 0f);
      default:
        return new Vector3(0, 0, 0);
    }
  }
}