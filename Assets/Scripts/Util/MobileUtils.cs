using System;
using UnityEngine;

namespace Keiwando.UI {

  public class MobileUtils {

    public static bool isProbablyMobilePhone() {
      #if UNITY_IOS || UNITY_ANDROID
      int width = Screen.currentResolution.width;
      int height = Screen.currentResolution.height;
      
      float widthInInches = width / Screen.dpi;
      float heightInInches = height / Screen.dpi;
      float averageSizeInInches = (float)Math.Sqrt(widthInInches * widthInInches + heightInInches * heightInInches);
      
      return averageSizeInInches < 6.5f;
      #else
      return false;
      #endif
    }
  }
}