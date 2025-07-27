#pragma description Evolution CreatureRecording file

#pragma endian little

import std.io;
import std.mem;
import std.string;
import std.math;
import std.bit;
import std.core;

fn to_string(auto var) {
  return str(var);
};

struct String {
  u32 length;
  char text[length];
} [[format("string_to_string")]];
fn string_to_string(auto var) {
  return var.text;
};

struct Vector2 {
  float x;
  float y;
} [[format("vector2_to_string")]];
fn vector2_to_string(auto var) {
  return std::format("({}, {})", var.x, var.y);
};

// CreatureDesign

enum CreatureDesignDataBlockType: s16 {
  Joints = 0,
  Bones = 1,
  Muscles = 2,
  Decorations = 3
};

bitfield JointDataFlags {
  weightSerialized : 1;
  fitnessPenaltySerialized : 1;
  padding: 14;
};

struct JointData {
  u16 dataLength;
  auto byteAfterJointData = $ + dataLength;

  JointDataFlags flags;
  s32 id;
  float positionX;
  float positionY;
  if (flags.weightSerialized) {
    float weight;
  }
  if (flags.fitnessPenaltySerialized) {
    float fitnessPenaltyForTouchingGround;
  }

  $ = byteAfterJointData;
};

bitfield BoneDataFlags {
  weightSerialized : 1;
  isWing : 1;
  inverted : 1;
  legacy : 1;
  padding : 12;
};

struct BoneData {
  u16 dataLength;
  auto byteAfterBoneData = $ + dataLength;

  BoneDataFlags flags;
  s32 id;
  s32 startJointID;
  s32 endJointID;
  if (flags.weightSerialized) {
    float weight;
  }

  $ = byteAfterBoneData;
};

bitfield MuscleDataFlags {
  strengthSerialized : 1;
  canExpand : 1;
  userIdSerialized : 1;
  padding: 13;
};

struct MuscleData {
  u16 dataLength;
  auto byteAfterMuscleData = $ + dataLength;

  MuscleDataFlags flags;
  s32 id;
  s32 startBoneID;
  s32 endBoneID;
  if (flags.strengthSerialized) {
    float strength;
  }
  if (flags.userIdSerialized) {
    String userID;
  }

  $ = byteAfterMuscleData;
};

bitfield DecorationDataFlags {
  flipX: 1;
  flipY: 1;
  padding: 14; 
};

enum DecorationType: s32 {
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
};

struct DecorationData {
  u16 dataLength;
  auto byteAfterDecorationData = $ + dataLength;

  DecorationDataFlags flags;
  s32 id;
  s32 boneId;
  float offsetX;
  float offsetY;
  float scale;
  float rotation;
  DecorationType decorationType;

  $ = byteAfterDecorationData;
};

struct CreatureDesignJointsData {
  u32 jointsCount;
  JointData joints[jointsCount];
};

struct CreatureDesignBonesData {
  u32 bonesCount;
  BoneData bones[bonesCount];
};

struct CreatureDesignMusclesData {
  u32 musclesCount;
  MuscleData muscles[musclesCount];
};

struct CreatureDesignDecorationsData {
  u32 decorationsCount;
  DecorationData decorations[decorationsCount];
};

struct CreatureDesignDataBlock {
  u32 blockLength;
  auto byteAfterBlock = $ + blockLength;
  CreatureDesignDataBlockType blockType;

  match (blockType) {
    (CreatureDesignDataBlockType::Joints) : CreatureDesignJointsData data [[inline]];
    (CreatureDesignDataBlockType::Bones) : CreatureDesignBonesData data [[inline]];
    (CreatureDesignDataBlockType::Muscles) : CreatureDesignMusclesData data [[inline]];
    (CreatureDesignDataBlockType::Decorations) : CreatureDesignDecorationsData data [[inline]];
  }

  $ = byteAfterBlock;
};

struct CreatureDesign {
  char signature[8];
  u16 version;
  
  u32 dataLength;
  auto expectedEndByte = $ + dataLength;
  
  String name;

  CreatureDesignDataBlock dataBlocks[while($ < expectedEndByte)];
};

// End CreatureDesign

struct CreatureRecordingMovementData {
  u32 dataLength;
  auto byteAfterMovementData = $ + dataLength;

  u16 flags;
  s32 sampleCount;

  float timestamps[sampleCount];

  s32 numberOfJoints;
  Vector2 jointPositions[numberOfJoints * sampleCount];

  s32 numberOfMuscles;
  float muscleForces[numberOfMuscles * sampleCount];

  $ = byteAfterMovementData;
};

struct SimulationSceneDescription {
  u16 binaryVersion;
  u32 dataLength;
  auto byteAfterSceneDescription = $ + dataLength;

  $ = byteAfterSceneDescription;
};

// End SimulationSceneDescription

enum Objective: u8 {
  Running = 0,
  Jumping = 1,
  ObstacleJump = 2,
  Climbing = 3,
  Flying = 4
};

struct CreatureStats {
  u32 dataLength;
  auto byteAfterData = $ + dataLength;

  u16 flags;
  float unclampedFitness;
  float fitness;
  s32 simulationTime;
  float horizontalDistanceTravelled;
  float verticalDistanceTravelled;
  float maxJumpHeight;
  float weight;
  s32 numberOfBones;
  s32 numberOfMuscles;
  float averageSpeed;

  $ = byteAfterData;
};

struct NeuralNetworkSettings {
  u32 dataLength;
  auto byteAfterData = $ + dataLength;
  u16 flags;
  s32 nodesPerIntermediateLayerLength;
  s32 nodesPerIntermediateLayer[nodesPerIntermediateLayerLength];
  $ = byteAfterData;
};

struct CreatureRecording {
  char signature[8];
  u16 version;

  u32 metadataBlockLength;
  auto byteAfterMetadataBlock = $ + metadataBlockLength;
  s32 generation;
  s64 encodedDate;
  Objective objective;
  CreatureStats stats;
  s32 networkInputCount;
  s32 networkOutputCount;
  NeuralNetworkSettings networkSettings;

  $ = byteAfterMetadataBlock;

  CreatureDesign creatureDesign;
  SimulationSceneDescription sceneDescription;
  CreatureRecordingMovementData movementData;
};

CreatureRecording recording @ 0x0 [[inline]];
