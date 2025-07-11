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

enum CreatureDesignDataBlockType: s16 {
  Joints = 0,
  Bones = 1,
  Muscles = 2
};

bitfield JointDataFlags {
  padding: 14;
  fitnessPenaltySerialized : 1;
  weightSerialized : 1;
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
  padding : 12;
  legacy : 1;
  inverted : 1;
  isWing : 1;
  weightSerialized : 1;
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
  padding : 13;
  userIdSerialized : 1;
  canExpand : 1;
  strengthSerialized : 1;
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

struct CreatureDesignDataBlock {
  u32 blockLength;
  auto byteAfterBlock = $ + blockLength;
  CreatureDesignDataBlockType blockType;

  match (blockType) {
    (CreatureDesignDataBlockType::Joints) : CreatureDesignJointsData data [[inline]];
    (CreatureDesignDataBlockType::Bones) : CreatureDesignBonesData data [[inline]];
    (CreatureDesignDataBlockType::Muscles) : CreatureDesignMusclesData data [[inline]];
  }

  $ = byteAfterBlock;
};

u128 g_byteAfterCreatureDesign = 0;

struct CreatureDesign {
  char signature[8];
  u16 version;
  String name;

  CreatureDesignDataBlock dataBlocks[while($ < g_byteAfterCreatureDesign)];
};

struct CreatureRecording {
  char signature[8];
  u16 version;
  s32 generation;
  s64 encodedDate;

  u32 creatureDesignBlockLength;
  g_byteAfterCreatureDesign = $ + creatureDesignBlockLength;
  CreatureDesign creatureDesign;

  SimulationSceneDescription sceneDescription;

  CreatureRecordingMovementData movementData;

  $ = g_byteAfterCreatureDesign;
};

CreatureRecording recording @ 0x0 [[inline]];
