using System;
using UnityEngine;

namespace Keiwando.Evolution {

  public abstract class ZoomableCamera : MonoBehaviour {

    /// <summary>
    /// The anchor around which zooming is performed relative to the camera bounds
    /// </summary>
    public Vector2 ZoomAnchor {
      get { return zoomAnchor; }
      set { zoomAnchor = value; }
    }
    [SerializeField]
    protected Vector2 zoomAnchor = new Vector2(0.5f, 0.5f);

    /// <summary>
    /// The amount by which the camera can be zoomed in.
    /// </summary>
    [SerializeField]
    protected float zoomInLength = 10;

    /// <summary>
    /// The amount by which the camera can be zoomed out
    /// </summary>
    [SerializeField]
    protected float zoomOutLength = 10;

    /// <summary>
    /// The difference between the lowest and highest zoom level (orthographicSize).
    /// </summary>
    private float zoomLength {
      get { return zoomInLength + zoomOutLength; }
    }

    /// <summary>
    /// The minimum allowed value of the camera's orthographic size
    /// </summary>
    protected float minZoom;

    private float initialZoom;

    public bool InteractiveZoomEnabled = true;

    public GameObject[] pointerHoverAreasToIgnore;

    private Camera _camera;

    internal virtual void Start(Camera camera) {
      this._camera = camera;
      initialZoom = camera.orthographicSize;
      minZoom = initialZoom - zoomInLength;

      InputRegistry.shared.Register(InputType.Scroll, this, EventHandleMode.PassthroughEvent);
      var scrollRecognizer = GestureRecognizerCollection.shared.GetScrollGestureRecognizer();
      scrollRecognizer.OnGesture += OnScrollGesture;

      InputRegistry.shared.Register(InputType.Touch, this, EventHandleMode.PassthroughEvent);
      var pinchRecognizer = GestureRecognizerCollection.shared.GetPinchGestureRecognizer();
      pinchRecognizer.OnGesture += OnPinchGesture;
    }

    void OnDestroy() {
      GestureRecognizerCollection gestureRecCollection = GestureRecognizerCollection.shared;
      if (gestureRecCollection != null) {
        gestureRecCollection.GetScrollGestureRecognizer().OnGesture -= OnScrollGesture;
        gestureRecCollection.GetPinchGestureRecognizer().OnGesture -= OnPinchGesture;
      }
    }

    private void OnScrollGesture(ScrollGestureRecognizer recognizer) {
      if (InputRegistry.shared.MayHandle(InputType.Scroll, this) && ZoomingIsAllowed()) {
        SetZoom(_camera.orthographicSize - recognizer.ScrollDelta.y);
      }  
    }

    private void OnPinchGesture(PinchGestureRecognizer recognizer) {
      if (InputRegistry.shared.MayHandle(InputType.Touch, this) && ZoomingIsAllowed()) {
        SetZoom(_camera.orthographicSize / Math.Max(0.0000001f, recognizer.ScaleDelta));
        OnPinch(recognizer);
      }
    }

    private bool ZoomingIsAllowed() {
      if (!InteractiveZoomEnabled) {
        return false;
      }
      if (pointerHoverAreasToIgnore != null) {
        foreach (GameObject pointerHoverArea in pointerHoverAreasToIgnore) {
          if (!pointerHoverArea.gameObject.activeInHierarchy) {
            continue;
          }
          bool isHovered = false;
          RectTransform rect = pointerHoverArea.transform as RectTransform;
          #if UNITY_IOS || UNITY_ANDROID 
          for (int touchI = 0; touchI < Input.touchCount; touchI++) {
            isHovered |= RectTransformUtility.RectangleContainsScreenPoint(rect, Input.GetTouch(touchI).position);
          }
          #else 
          isHovered = RectTransformUtility.RectangleContainsScreenPoint(rect, Input.mousePosition);
          #endif
          if (isHovered) {
            return false;
          }
        }
      }
      return true;
    }

    private void SetZoom(float newZoom) {

      var visibleSize = CameraUtils.GetOrthographicSize(_camera);

      var size = _camera.orthographicSize;
      var newSize = Math.Max(minZoom, Math.Min(minZoom + zoomLength, newZoom));
      _camera.orthographicSize = newSize;

      var dSize = newSize / size;
      var dPercent = dSize - 1f;

      // Readjust the center position based on the zoom anchor
      var anchorAdjustX = (zoomAnchor.x - 0.5f) * visibleSize.x;
      var anchorAdjustY = (zoomAnchor.y - 0.5f) * visibleSize.y;

      var pos = _camera.transform.position;
      pos.x += anchorAdjustX;
      pos.y += anchorAdjustY;
      _camera.transform.position = pos;

      OnAfterZoom();
    }

    public float GetCurrentRelativeZoom() {
      return _camera.orthographicSize / initialZoom;
    }

    protected abstract void OnAfterZoom();
    protected virtual void OnPinch(PinchGestureRecognizer gestureRecognizer) { }
  }
}