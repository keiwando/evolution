using System;
using UnityEngine;
using Keiwando.Evolution.Scenes;

public class CreatureRecordingMovementData {
  public float[] sampleTimestamps;
  public Vector2[,] jointPositions;
  public float[,] muscleForces;

  public CreatureRecordingMovementData(int validSampleCount, int numberOfJoints, int numberOfMuscles) {
      sampleTimestamps = new float[validSampleCount];
      jointPositions = new Vector2[numberOfJoints, validSampleCount];
      muscleForces = new float[numberOfMuscles, validSampleCount];
  }
}

public class CreatureRecording {

  public CreatureDesign creatureDesign;
  public SimulationSceneDescription sceneDescription;
  public CreatureRecordingMovementData movementData;
  public int generation;
  public DateTime date;

  public CreatureRecording(CreatureDesign creatureDesign,
                           SimulationSceneDescription sceneDescription,
                           CreatureRecordingMovementData movementData,
                           int generation) {
    this.creatureDesign = creatureDesign;
    this.generation = generation;
    this.sceneDescription = sceneDescription;
    this.movementData = movementData;
    this.date = DateTime.Now;
  }
}

public class CreatureRecorder {

  public const int SAMPLES_PER_SECOND = 30;
  public const float SECONDS_PER_SAMPLE = 1.0f / SAMPLES_PER_SECOND;

  public float[] sampleTimestamps;
  public Vector2[,] jointPositions;
  public float[,] muscleForces;

  private int sampleCapacity;
  private int validSampleCount = 0;

  private int nextSampleIndexToRecord = 0;
  private float firstSampleTime = 0;
  private float currentSampleTime = 0;

  public int recordingDurationInSeconds {
    get { return _recordingDurationInSeconds; }
  }
  private int _recordingDurationInSeconds;
  public int numberOfJoints {
    get { return jointPositions.GetLength(0); }
  }
  public int numberOfMuscles {
    get { return muscleForces.GetLength(0); }
  }

  public CreatureRecorder(int recordingDurationInSeconds, 
                           int numberOfJoints,
                           int numberOfMuscles) {
    int sampleCount = SAMPLES_PER_SECOND * recordingDurationInSeconds;
    this.sampleCapacity = sampleCount;
    this.sampleTimestamps = new float[sampleCount];
    this.jointPositions = new Vector2[numberOfJoints, sampleCount];
    this.muscleForces = new float[numberOfMuscles, sampleCount];

    reset();
  }

  public void reset() {

    for (int i = 0; i < sampleTimestamps.Length; i++) {
      sampleTimestamps[i] = 0f;
    }
    for (int jointIndex = 0; jointIndex < jointPositions.GetLength(0); jointIndex++) {
      for (int sampleIndex = 0; sampleIndex < jointPositions.GetLength(1); sampleIndex++) {
        jointPositions[jointIndex, sampleIndex] = new Vector2(0, 0);
      }
    }
    for (int i = 0; i < muscleForces.GetLength(0); i++) {
      for (int j = 0; j < muscleForces.GetLength(1); j++) {
        muscleForces[i, j] = 0f;
      }
    }
    validSampleCount = 0;
    nextSampleIndexToRecord = 0;
    firstSampleTime = 0;
    currentSampleTime = 0;
  }

  public bool shouldRecordNewSample() {
    return Time.time - currentSampleTime > 0.8f * SECONDS_PER_SAMPLE;
  }

  public void beginRecordingSample() {
    if (nextSampleIndexToRecord >= sampleCapacity) {
      return;
    }
    currentSampleTime = Time.time;
    if (nextSampleIndexToRecord == 0) {
      firstSampleTime = currentSampleTime;
    }
    sampleTimestamps[nextSampleIndexToRecord] = currentSampleTime - firstSampleTime;
  }

  public void recordJointPosition(int jointIndex, Vector2 jointPosition) {
    if (nextSampleIndexToRecord >= sampleCapacity) {
      return;
    }
    if (jointIndex < 0 || jointIndex >= jointPositions.GetLength(0)) {
      Debug.Log($"Invalid joint index {jointIndex} for recording");
      return;
    }

    jointPositions[jointIndex, nextSampleIndexToRecord] = jointPosition;
  }

  public void recordMuscleForce(int muscleIndex, float force) {
    if (nextSampleIndexToRecord >= sampleCapacity) {
      return;
    }
    if (muscleIndex < 0 || muscleIndex >= muscleForces.GetLength(0)) {
      Debug.Log($"Invalid muscle index {muscleIndex} for recording");
      return;
    }

    muscleForces[muscleIndex, nextSampleIndexToRecord] = force;
  }

  public void endRecordingSample() {
    nextSampleIndexToRecord += 1;
    if (validSampleCount < sampleCapacity) {
      validSampleCount += 1;
    }
  }

  public CreatureRecordingMovementData toRecordingMovementData() {
    CreatureRecordingMovementData recording = new CreatureRecordingMovementData(
      validSampleCount: validSampleCount,
      numberOfJoints: jointPositions.GetLength(0),
      numberOfMuscles: muscleForces.GetLength(0)
    );
    Array.Copy(sampleTimestamps, recording.sampleTimestamps, validSampleCount);
    for (int jointIndex = 0; jointIndex < jointPositions.GetLength(0); jointIndex++) {
      for (int sampleIndex = 0; sampleIndex < validSampleCount; sampleIndex++) {
        recording.jointPositions[jointIndex, sampleIndex] = jointPositions[jointIndex, sampleIndex];
      }
    }
    for (int muscleIndex = 0; muscleIndex < muscleForces.GetLength(0); muscleIndex++) {
      for (int sampleIndex = 0; sampleIndex < validSampleCount; sampleIndex++) {
        recording.muscleForces[muscleIndex, sampleIndex] = muscleForces[muscleIndex, sampleIndex];
      }
    }
    return recording;
  }
}

public class CreatureRecordingPlayer {
  private int currentPlaybackSample = 0;
  private float currentPlaybackSampleInterpolationT = 0f;

  private float playbackStartTime = 0.0f;
  
  private CreatureRecordingMovementData recording;

  public CreatureRecordingPlayer(CreatureRecordingMovementData recording) {
    this.recording = recording;
  }

  public void beginPlayback() {
    playbackStartTime = Time.time;
  }

  public void seekPlaybackToAbsoluteTime(float time) {
    seekPlaybackToTime(time - playbackStartTime);
  }

  public void seekPlaybackToTime(float playbackTime) {
    int validSampleCount = recording.sampleTimestamps.Length;
    if (validSampleCount <= 0) {
      return;
    }

    float validRecordingDurationInSeconds = recording.sampleTimestamps[validSampleCount - 1];
    float wrappedPlaybackTime = playbackTime % validRecordingDurationInSeconds;
    if (wrappedPlaybackTime < recording.sampleTimestamps[currentPlaybackSample]) {
      currentPlaybackSample = 0;
      currentPlaybackSampleInterpolationT = 0f;
    }
    for (int sampleIndex = currentPlaybackSample + 1; sampleIndex < validSampleCount; sampleIndex++) {
      float sampleTimestamp = recording.sampleTimestamps[sampleIndex];
      if (sampleTimestamp > wrappedPlaybackTime) {
        currentPlaybackSample = sampleIndex - 1;
        float previousTimestamp = recording.sampleTimestamps[sampleIndex - 1];
        float timeBetweenSamples = sampleTimestamp - previousTimestamp;
        if (timeBetweenSamples > 0.0001f) {
          currentPlaybackSampleInterpolationT = Math.Clamp((wrappedPlaybackTime - previousTimestamp) / timeBetweenSamples, 0f, 1f);
        } else {
          currentPlaybackSampleInterpolationT = 0f;
        }
        
        return;
      }
    }
  }

  public Vector2 getRecordedJointPosition(int jointIndex) {
    Vector2 currentJointPosition = recording.jointPositions[jointIndex, currentPlaybackSample];
    int validSampleCount = recording.sampleTimestamps.Length;
    if (currentPlaybackSample + 1 < validSampleCount) {
      Vector2 nextJointPosition = recording.jointPositions[jointIndex, currentPlaybackSample + 1];
      return Vector2.Lerp(currentJointPosition, nextJointPosition, currentPlaybackSampleInterpolationT);
    } else {
      return currentJointPosition;
    }
  }

  public float getRecordedMuscleForce(int muscleIndex) {
    return recording.muscleForces[muscleIndex, currentPlaybackSample];
  }
}