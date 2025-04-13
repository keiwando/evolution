public class Easing {

  public static float EaseInQuad(float t) {
    return t * t;
  }

  public static float EaseOutQuad(float t) {
    return t * (2 - t);
  }

  public static float EaseInOutQuad(float t) {
    return t < 0.5 ? 2 * t * t : (4 - 2 * t) * t - 1;
  }

  public static float EaseInCubic(float t) {
    return t * t * t;
  }

  public static float EaseOutCubic(float t){
    float s = t - 1;
    return s * s * s + 1; // y = (x - 1)^3 + 1
  }

  public static float EaseInOutCubic(float t) {
    return t < 0.5 ? 4 * t * t * t : (t - 1) * (2 * t - 2) * (2 * t - 2) + 1;
  }

  public static float EaseInQuart(float t) {
    return t * t * t * t;
  }

  public static float EaseOutQuart(float t) {
    float s = t - 1;
    return s * s * s * (1 - t) + 1; // y = 1 - (x - 1)^4
  }

  public static float EaseInOutQuart(float t) {
     if (t < 0.5) {
      return 8 * t * t * t * t;
    } else {
      // y = (1/2)((2x)^4)        ; [0, 0.5)
      // y = -(1/2)((2x-2)^4 - 2) ; [0.5, 1]
      float s = t - 1;
      return -8 * s * s * s * s + 1;
    }
  }

  public static float EaseInQuint(float t) {
    return t * t * t * t * t;
  }

  public static float EaseOutQuint(float t) {
    float s = t - 1f;
    return s * s * s * s * s + 1; // y = (x - 1)^5 + 1
  }

  public static float EaseInOutQuint(float t) {
    if (t < 0.5) {
      return 16f * t * t * t * t * t;
    } else {
      // y = (1/2)((2x)^5)       ; [0, 0.5)
      // y = (1/2)((2x-2)^5 + 2) ; [0.5, 1]
      float s = (2f * t) - 2f;
      return 0.5f * s * s * s * s * s + 1;
    }
  }
}