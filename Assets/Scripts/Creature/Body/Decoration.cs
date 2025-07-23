using System;
using System.Collections.Generic;
using UnityEngine;

public class Decoration : BodyComponent {

  private const string GOOGLY_EYE_PREFAB_PATH = "Prefabs/Googly Eye";
  private const string SPRITE_DECORATION_PREFAB_PATH = "Prefabs/Decoration";

  public const float DEFAULT_GOOGLY_EYE_SCALE = 1.3f;

  private const float Z_POSITION = -1f;

  public Bone bone;  

  public Vector3 bindPoint => bone.Center;

  public DecorationData DecorationData { get; set; }

  public bool VisualizeConnection { get; private set; }

  SpriteRenderer spriteRenderer;
  // GooglyEye googlyEye;
  SpriteRenderer[] googlyEyeSpriteRenderers;
  LineRenderer connectionVisualizationRenderer;

  public static Decoration CreateFromData(Bone bone, DecorationData data) {

    string prefabPath = data.decorationType == DecorationType.GooglyEye ? GOOGLY_EYE_PREFAB_PATH : SPRITE_DECORATION_PREFAB_PATH;
    var decoration = ((GameObject) Instantiate(Resources.Load(prefabPath), bone.Center + new Vector3(data.offset.x, data.offset.y, Z_POSITION), Quaternion.identity)).GetComponent<Decoration>();
    decoration.DecorationData = data;
    decoration.bone = bone;
    decoration.spriteRenderer = decoration.GetComponent<SpriteRenderer>();
    decoration.connectionVisualizationRenderer = decoration.GetComponent<LineRenderer>();
    decoration.connectionVisualizationRenderer.enabled = false;

    decoration.UpdateOrientation();

    if (data.decorationType == DecorationType.GooglyEye) {
      // decoration.googlyEye = decoration.GetComponent<GooglyEye>();
      decoration.googlyEyeSpriteRenderers = decoration.GetComponentsInChildren<SpriteRenderer>();
    }
    return decoration;
  }

  public void UpdateOrientation() {
    float scale = DecorationData.scale;
    transform.position = bone.transform.TransformPoint(new Vector3(DecorationData.offset.x, DecorationData.offset.y, Z_POSITION));
    transform.rotation = bone.transform.rotation * Quaternion.Euler(0f, 0f, DecorationData.rotation * Mathf.Rad2Deg);
    transform.localScale = new Vector3(scale, scale, scale);

    if (connectionVisualizationRenderer != null && VisualizeConnection) {
      connectionVisualizationRenderer.SetPosition(0, transform.position);
      connectionVisualizationRenderer.SetPosition(1, bone.Center);
    }
  }

  public void SetVisualizeConnection(bool visualize) {
    this.VisualizeConnection = visualize;
    if (connectionVisualizationRenderer != null) {
      connectionVisualizationRenderer.enabled = visualize;
    }
    UpdateOrientation();
  }

  public void SetLayer(LayerMask layer) {
    this.gameObject.layer = layer;
    if (googlyEyeSpriteRenderers != null) {
      foreach (SpriteRenderer spriteRenderer in googlyEyeSpriteRenderers) {
        spriteRenderer.gameObject.layer = layer;
      }
    }
  }

  protected override void SetRendererMaterialForHighlight(Material mat, bool selected) {
    if (googlyEyeSpriteRenderers != null) {
      foreach (SpriteRenderer spriteRenderer in googlyEyeSpriteRenderers) {
        spriteRenderer.material = mat;
      }
    }
    SetVisualizeConnection(selected);
  }

  public override void Delete() {
    base.Delete();
    Destroy(gameObject);
  }

  public override void PrepareForEvolution() {}
  
  public override int GetId() {
    return DecorationData.id;
  }
}