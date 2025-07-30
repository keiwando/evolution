using System;
using Keiwando.Evolution;
using UnityEngine;

// TODO: Add controls for changing the sorting order of decorations

public class TransformGizmo: MonoBehaviour {

  private const float BASE_HANDLE_SCALE = 0.5f;
  public const float BASE_HANDLE_DISTANCE_FROM_ORIGIN = 2f;

  public SpriteRenderer scaleHandle;
  public SpriteRenderer rotationHandle;
  [SerializeField] private LineRenderer scaleLineRenderer;
  [SerializeField] private LineRenderer rotationLineRenderer;
  [SerializeField] private ZoomableCamera zoomableCamera;

  public float currentScaleFactor = 1f;

  public void UpdateOrientation() {

    scaleHandle.gameObject.transform.position = this.transform.position + currentScaleFactor * BASE_HANDLE_DISTANCE_FROM_ORIGIN * transform.up;
    rotationHandle.gameObject.transform.position = this.transform.position + currentScaleFactor * BASE_HANDLE_DISTANCE_FROM_ORIGIN * transform.right;

    scaleLineRenderer.SetPosition(0, this.transform.position);
    scaleLineRenderer.SetPosition(1, scaleHandle.transform.position);

    rotationLineRenderer.SetPosition(0, this.transform.position);
    rotationLineRenderer.SetPosition(1, rotationHandle.transform.position);

    float handleScale = BASE_HANDLE_SCALE * zoomableCamera.GetCurrentRelativeZoom();
    scaleHandle.gameObject.transform.localScale = new Vector3(handleScale, handleScale, handleScale);
    rotationHandle.gameObject.transform.localScale = new Vector3(handleScale, handleScale, handleScale);
  }

  public void Update() {
    UpdateOrientation();
  }

  public void Reset() {
    this.currentScaleFactor = 1f;
    this.transform.rotation = Quaternion.identity;
    UpdateOrientation();
  }

  public bool IsPointerOverScaleHandle(Vector3 mousePositionInWorldSpace) {
    return IsPointerOverHandle(mousePositionInWorldSpace, scaleHandle.gameObject);    
  }

  public bool IsPointerOverRotationHandle(Vector3 mousePositionInWorldSpace) {
    return IsPointerOverHandle(mousePositionInWorldSpace, rotationHandle.gameObject);
  }

  private bool IsPointerOverHandle(Vector3 mousePositionInWorldSpace, GameObject handle) {
    Vector2 mousePositionIn2D = new Vector2(mousePositionInWorldSpace.x, mousePositionInWorldSpace.y);
    Vector2 handlePositionIn2D = new Vector2(handle.transform.position.x, handle.transform.position.y);
    float distanceToHandle = Vector2.Distance(mousePositionIn2D, handlePositionIn2D);
    return distanceToHandle < handle.transform.lossyScale.x * 2f;
  } 
}