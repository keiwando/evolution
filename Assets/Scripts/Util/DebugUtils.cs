using UnityEngine;

public class DebugUtils {

  // Note: This sets depthTest to false by default, as opposed to Debug.DrawLine which sets it to true.
  public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0.0f, bool depthTest = false) {
    Debug.DrawLine(start, end, color, duration, depthTest);
  }

  public static void DrawCross(Vector3 worldCenter, float worldSize, Color color, float duration = 0.0f, bool depthTest = false) {
    Vector3 left = worldCenter + new Vector3(-0.5f * worldSize, 0, 0);
    Vector3 right = worldCenter + new Vector3(0.5f * worldSize, 0, 0);
    Vector3 top = worldCenter + new Vector3(0, -0.5f * worldSize, 0);
    Vector3 bottom = worldCenter + new Vector3(0, 0.5f * worldSize, 0);
    Debug.DrawLine(left, right, color, duration, depthTest);
    Debug.DrawLine(top, bottom, color, duration, depthTest);
  }
}