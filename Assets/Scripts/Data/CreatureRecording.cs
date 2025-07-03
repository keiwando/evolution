using UnityEngine;
using System;

public class CreatureRecording {

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

  private int currentPlaybackSample = 0;
  private float currentPlaybackSampleInterpolationT = 0f;

  public CreatureRecording(int recordingDurationInSeconds, 
                           int numberOfJoints,
                           int numberOfMuscles) {
    int sampleCount = SAMPLES_PER_SECOND * recordingDurationInSeconds;
    this.sampleCapacity = sampleCount;
    this.sampleTimestamps = new float[sampleCount];

    this.jointPositions = new Vector2[numberOfJoints, sampleCount];
    for (int jointIndex = 0; jointIndex < numberOfJoints; jointIndex++) {
      for (int sampleIndex = 0; sampleIndex < sampleCount; sampleIndex++) {
        jointPositions[jointIndex, sampleIndex] = new Vector2(0, 0);
      }
    }
    this.muscleForces = new float[numberOfMuscles, sampleCount];
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

  public void seekPlaybackToTime(float playbackTime) {
    if (validSampleCount <= 0) {
      return;
    }

    float validRecordingDurationInSeconds = this.sampleTimestamps[validSampleCount - 1];
    float wrappedPlaybackTime = playbackTime % validRecordingDurationInSeconds;
    if (wrappedPlaybackTime < sampleTimestamps[currentPlaybackSample]) {
      currentPlaybackSample = 0;
      currentPlaybackSampleInterpolationT = 0f;
    }
    for (int sampleIndex = currentPlaybackSample + 1; sampleIndex < validSampleCount; sampleIndex++) {
      float sampleTimestamp = sampleTimestamps[sampleIndex];
      if (sampleTimestamp > wrappedPlaybackTime) {
        currentPlaybackSample = sampleIndex - 1;
        float previousTimestamp = sampleTimestamps[sampleIndex - 1];
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
    Vector2 currentJointPosition = jointPositions[jointIndex, currentPlaybackSample];
    if (currentPlaybackSample + 1 < validSampleCount) {
      Vector2 nextJointPosition = jointPositions[jointIndex, currentPlaybackSample + 1];
      return Vector2.Lerp(currentJointPosition, nextJointPosition, currentPlaybackSampleInterpolationT);
    } else {
      return currentJointPosition;
    }
  }

  public float getRecordedMuscleForce(int muscleIndex) {
    return muscleForces[muscleIndex, currentPlaybackSample];
  }
}