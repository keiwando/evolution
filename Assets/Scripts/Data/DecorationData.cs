using System;
using UnityEngine;
using Keiwando.JSON;

public enum DecorationType: int {
  GooglyEye = 0,
  ShiftyEyes = 1,
  EmptyEyeOval = 2,
  EmptyEyeCircle = 3,
  EmptyEyeRounded = 4,
  EmptyEyeSlanted = 5,
  EmptyEyeHappy = 6,
  PupilDot = 7,
  PupilTriangleCut = 8,
  PupilReflection = 9,
  EyebrowNormal = 10,
  EyebrowRaised = 11,
  EyebrowAngry = 12,
  MouthSmile = 13,
  MouthLaugh = 14,
  MouthLipSmile = 15,
  MouthWorried = 16,
  MouthLips = 17,
  MouthLipsSmall = 18,
  MouthTongue = 19,
  NoseBigFront = 20,
  NoseRaised = 21,
  NoseSmall = 22,
  NoseCrooked = 23,
  NoseDroopy = 24,
  Moustache1 = 25,
  Moustache2 = 26,
  Moustache3 = 27,
  EarSide = 28,
  EarFront = 29,
  HandOpenPalm = 30,
  HandBack = 31,
  Fist = 32,
  Foot = 33,
  Shoe = 34,
  ShoeHighHeel = 35,
  ShoeCartoon = 36,
  Brain = 37,
  Bone = 38,
  CatEar = 39,
  CatWhiskers = 40,
  CatSnout = 41
  // Note: When adding more cases here, update DecorationUtils.MAX_VALUE_RAW_VALUE
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
    Vector2 offset,
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

  public static int MAX_VALID_RAW_VALUE = 41;

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
          decorationEmojiSprites = Resources.LoadAll<Sprite>("Sprites/Decorations/skin_components");
        }
        return decorationEmojiSprites[index];
    }
  } 

  public static Vector3 HitboxSizeForDecoration(DecorationType type) {
    switch (type) {
      case DecorationType.GooglyEye: // Note: This one is unused, since it's a manually defined sphere collider.
        return new Vector3(0.55f, 0.55f, 0.55f);
      case DecorationType.EmptyEyeOval:
        return new Vector3(1.36f, 2.24f, 0.2f);
      case DecorationType.EmptyEyeRounded:
        return new Vector3(1.2f, 2.0f, 0.2f);
      case DecorationType.EmptyEyeSlanted:
        return new Vector3(1.36f, 1.52f, 0.2f);
      case DecorationType.EmptyEyeHappy:
        return new Vector3(1.56f, 1.28f, 0.2f);
      case DecorationType.PupilDot:
        return new Vector3(0.68f, 0.68f, 0.2f);
      case DecorationType.PupilTriangleCut:
        return new Vector3(0.88f, 1.44f, 0.2f);
      case DecorationType.PupilReflection:
        return new Vector3(0.88f, 0.92f, 0.2f);
      case DecorationType.EyebrowNormal:
        return new Vector3(1.72f, 0.64f, 0.2f);
      case DecorationType.EyebrowRaised:
        return new Vector3(1.5f, 1.24f, 0.2f);
      case DecorationType.EyebrowAngry:
        return new Vector3(1.4f, 1f, 0.2f);
      case DecorationType.MouthSmile:
        return new Vector3(2f, 0.4f, 0.2f);
      case DecorationType.MouthLaugh:
        return new Vector3(1.73f, 1f, 0.2f);
      case DecorationType.MouthLipSmile:
        return new Vector3(2f, 1.34f, 0.2f);
      case DecorationType.MouthWorried:
        return new Vector3(1.72f, 0.92f, 0.2f);
      case DecorationType.MouthLips:
        return new Vector3(1.44f, 0.89f, 0.2f);
      case DecorationType.MouthLipsSmall:
        return new Vector3(1.44f, 0.44f, 0.2f);
      case DecorationType.MouthTongue:
        return new Vector3(2f, 1.08f, 0.2f);
      case DecorationType.NoseBigFront:
        return new Vector3(1.17f, 1.3f, 0.2f);
      case DecorationType.NoseRaised:
        return new Vector3(1.23f, 0.82f, 0.2f);
      case DecorationType.NoseSmall:
        return new Vector3(0.5f, 0.8f, 0.2f);
      case DecorationType.NoseCrooked:
        return new Vector3(0.94f, 1f, 0.2f);
      case DecorationType.NoseDroopy:
        return new Vector3(0.56f, 1.2f, 0.2f);
      case DecorationType.Moustache1:
        return new Vector3(1.7f, 0.5f, 0.2f);
      case DecorationType.Moustache2:
        return new Vector3(2f, 0.56f, 0.2f);
      case DecorationType.Moustache3:
        return new Vector3(2f, 0.85f, 0.2f);
      case DecorationType.EarSide:
        return new Vector3(1.12f, 1.85f, 0.2f);
      case DecorationType.EarFront:
        return new Vector3(0.64f, 2f, 0.2f);
      case DecorationType.HandOpenPalm:
        return new Vector3(2f, 1.52f, 0.2f);
      case DecorationType.HandBack:
        return new Vector3(2f, 1.24f, 0.2f);
      case DecorationType.Fist:
        return new Vector3(1.55f, 1.16f, 0.2f);
      case DecorationType.Foot:
        return new Vector3(2f, 0.8f, 0.2f);
      case DecorationType.Shoe:
        return new Vector3(2.12f, 0.88f, 0.2f);
      case DecorationType.ShoeCartoon:
        return new Vector3(2f, 0.94f, 0.2f);
      case DecorationType.Brain:
        return new Vector3(2.04f, 1.6f, 0.2f);
      case DecorationType.Bone:
        return new Vector3(2f, 0.68f, 0.2f);
      default:
        return new Vector3(2.0f, 2.0f, 0.2f);
    }
  }

  public static Vector3 HitboxCenterForDecoration(DecorationType type) {
    switch (type) {
      case DecorationType.NoseRaised:
        return new Vector3(-0.06f, -0.12f, 0f);
      case DecorationType.NoseSmall:
        return new Vector3(-0.06f, 0f, 0f);
      case DecorationType.Moustache1: 
        return new Vector3(-0f, -0.12f, 0f);
      case DecorationType.Moustache2:
        return new Vector3(0f, -0.08f, 0f);
      case DecorationType.Moustache3:
        return new Vector3(0f, -0.12f, 0f);
      case DecorationType.EarSide:
        return new Vector3(-0.21f, -0.09f, 0f);
      case DecorationType.HandBack:
        return new Vector3(0f, -0.04f, 0f);
      default:
        return new Vector3(0, 0, 0);
    }
  }

  public static float DefaultScaleForDecoration(DecorationType type) {
    switch (type) {
      case DecorationType.GooglyEye:
        return 2.0f;
      case DecorationType.ShiftyEyes:
        return 1.2f;
      case DecorationType.MouthSmile:
      case DecorationType.MouthLaugh:
      case DecorationType.MouthLipSmile:
      case DecorationType.MouthWorried:
      case DecorationType.MouthLips:
      case DecorationType.MouthLipsSmall:
      case DecorationType.MouthTongue:
      case DecorationType.NoseBigFront:
      case DecorationType.NoseRaised:
      case DecorationType.NoseSmall:
      case DecorationType.NoseCrooked: 
      case DecorationType.NoseDroopy:
      case DecorationType.EarSide:
      case DecorationType.EarFront:
      case DecorationType.Brain:
      case DecorationType.Bone:
      case DecorationType.HandOpenPalm:
      case DecorationType.HandBack:
      case DecorationType.Fist:
      case DecorationType.Foot:
      case DecorationType.Shoe:
      case DecorationType.ShoeHighHeel:
      case DecorationType.ShoeCartoon:
        return 2.0f;
      case DecorationType.Moustache1:
      case DecorationType.Moustache2:
      case DecorationType.Moustache3:
        return 3.0f;
      default:
        return 1.4f;
    }
  }
}