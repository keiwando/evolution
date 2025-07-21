#pragma description Evolution SimulationData file

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

// CreatureDesign

enum CreatureDesignDataBlockType: s16 {
  Joints = 0,
  Bones = 1,
  Muscles = 2
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

struct CreatureDesign {
  char signature[8];
  u16 version;
  
  u32 dataLength;
  auto expectedEndByte = $ + dataLength;
  
  String name;

  CreatureDesignDataBlock dataBlocks[while($ < expectedEndByte)];
};

// End CreatureDesign

struct SimulationSceneDescription {
  u16 binaryVersion;
  u32 dataLength;
  auto byteAfterSceneDescription = $ + dataLength;

  $ = byteAfterSceneDescription;
};

enum Objective: u8 {
  Running = 0,
  Jumping = 1,
  ObstacleJump = 2,
  Climbing = 3,
  Flying = 4
};

enum SelectionAlgorithm: u8 {
  Uniform = 0,
  FitnessProportional = 1,
  TournamentSelection = 2,
  RankProportional = 3
};

enum RecombinationAlgorithm: u8 {
  OnePointCrossover = 0,
  MultiPointCrossover = 1,
  UniformCrossover = 2
};

enum MutationAlgorithm: u8 {
  Chunk = 0,
  Global = 1,
  Inversion = 2
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

struct SimulationSettings {
  u32 dataLength;
  auto endByte = $ + dataLength;

  u16 flags;
  bool keepBestCreatures;
  s32 simulationTime;
  s32 populationSize;
  bool simulateInBatches;
  s32 batchSize;
  Objective objective;
  float mutationRate;
  SelectionAlgorithm selectionAlgorithm;
  RecombinationAlgorithm recombinationAlgorithm;
  MutationAlgorithm mutationAlgorithm;

  $ = endByte;
};

s32 g_ChromosomeLength;
struct ChromosomeWeights {
  float weights[g_ChromosomeLength];
};

struct ChromosomeData {
  ChromosomeWeights chromosome;
  CreatureStats stats;
};

struct SimulationData {
  char signature[8];
  u16 version;

  u32 dataLengthBeforeChromosomes;
  auto startByteOfChromosomes = $ + dataLengthBeforeChromosomes;

  s16 simulationDataVersion;
  SimulationSettings simulationSettings;
  NeuralNetworkSettings networkSettings;
  CreatureDesign creatureDesign;
  SimulationSceneDescription sceneDescription;
  s32 lastV2SimulatedGeneration;

  $ = startByteOfChromosomes;

  u32 populationDataLength;
  auto byteAfterPopulationData = $ + populationDataLength;

  s32 chromosomeLength;
  g_ChromosomeLength = chromosomeLength;

  ChromosomeWeights populationChromosomes[simulationSettings.populationSize];

  $ = byteAfterPopulationData;

  s32 numberOfBestCreaturesData;
  ChromosomeData bestCreatureData[while($ < std::mem::size())];
};

SimulationData simulationData @ 0x0 [[inline]];
