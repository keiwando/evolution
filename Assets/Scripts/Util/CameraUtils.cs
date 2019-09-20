using UnityEngine;

public static class CameraUtils {

    /// <summary>
    /// Returns the orthographic viewport size in world coordinates.
    /// </summary>
    public static Vector2 GetOrthographicSize(Camera camera) {
        var halfHeight = camera.orthographicSize;
        var halfWidth = camera.aspect * halfHeight;
        return new Vector2(halfWidth * 2, halfHeight * 2);
    } 

    public static Vector3 ScreenToWorldDistance(Camera camera, Vector3 distance) {

		var p1 = camera.ScreenToWorldPoint(new Vector3(0,0,0));
		var p2 = camera.ScreenToWorldPoint(distance);

		return p2 - p1;
	}
}