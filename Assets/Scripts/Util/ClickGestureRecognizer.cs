#pragma warning disable CS0067
using UnityEngine;

namespace Keiwando {

  public class ClickGestureRecognizer: MonoBehaviour, IGestureRecognizer<ClickGestureRecognizer> {

    public event GestureCallback<ClickGestureRecognizer> OnGesture;

    public GestureRecognizerState State { get; private set; } = GestureRecognizerState.Possible;

    public Vector2 ClickPosition { get; private set; } = Vector2.zero;

    private float clickStartTime = 0f;

    private const float CLICK_DURATION_THRESHOLD = 0.8f;
    private const float CLICK_MOVE_THRESHOLD = 50f;

    void Update() {
      
      // We defer the reset of this to the next frame to allow other code to check the state
      // on the frame where a valid click was detected.
      if (this.State == GestureRecognizerState.Ended) {
        State = GestureRecognizerState.Possible;
      }

      #if UNITY_IOS || UNITY_ANDROID 
      if (Input.touchCount > 1) {
        this.State = GestureRecognizerState.Cancelled;
      }
      #endif

      if (InputUtils.MouseDown()) {
        this.ClickPosition = Input.mousePosition;
        this.clickStartTime = Time.time;
      }
      if (InputUtils.MouseHeld()) {
        float clickDuration = Time.time - clickStartTime;
        if (clickDuration > CLICK_DURATION_THRESHOLD) {
          this.State = GestureRecognizerState.Cancelled;
        }
        if (Vector2.Distance(Input.mousePosition, this.ClickPosition) > CLICK_MOVE_THRESHOLD) {
          this.State = GestureRecognizerState.Cancelled;
        }
      } 
      if (InputUtils.MouseUp()) {
        if (this.State == GestureRecognizerState.Possible) {
          this.State = GestureRecognizerState.Ended;
          if (OnGesture != null) OnGesture(this);
        } else {
          this.State = GestureRecognizerState.Possible;
        }
      }
    }

    public bool ClickEndedOnThisFrame() {
      return this.State == GestureRecognizerState.Ended;
    }
  }
}