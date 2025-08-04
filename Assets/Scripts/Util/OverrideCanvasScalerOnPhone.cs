using UnityEngine;
using UnityEngine.UI;

namespace Keiwando.UI {

  [RequireComponent(typeof(CanvasScaler))]
  public class OverrideCanvasScalerOnPhone: MonoBehaviour {

    public Vector2 referenceResolution = new Vector2(500, 500);

    void Start() {
      if (MobileUtils.isProbablyMobilePhone()) {
        CanvasScaler scaler = GetComponent<CanvasScaler>();
        scaler.referenceResolution = referenceResolution;
      }
    }
  }
}