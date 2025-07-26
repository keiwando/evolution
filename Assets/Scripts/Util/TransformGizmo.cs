using System;
using UnityEngine;

public class TransformGizmo: MonoBehaviour {

  public SpriteRenderer scaleHandle;
  public SpriteRenderer rotationHandle;
  [SerializeField] private LineRenderer scaleLineRenderer;
  [SerializeField] private LineRenderer rotationLineRenderer;

  public void UpdateOrientation() {

    scaleLineRenderer.SetPosition(0, this.transform.position);
    scaleLineRenderer.SetPosition(1, scaleHandle.transform.position);

    rotationLineRenderer.SetPosition(0, this.transform.position);
    rotationLineRenderer.SetPosition(1, rotationHandle.transform.position);
  }

  public void Update() {
    UpdateOrientation();
  }
}