using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Decoration : BodyComponent {

  private const string GOOGLY_EYE_PREFAB_PATH = "Prefabs/Googly Eye";
  private const string SPRITE_DECORATION_PREFAB_PATH = "Prefabs/Decoration";

  public const float DEFAULT_GOOGLY_EYE_SCALE = 1.3f;
  public const float DEFAULT_EMOJI_SCALE = 2.0f;

  private const float Z_POSITION = -1f;

  public Bone bone;  

  public Vector3 bindPoint => bone.Center;

  public DecorationData DecorationData { get; set; }

  public bool VisualizeConnection { get; private set; }

  SpriteRenderer spriteRenderer;
  // GooglyEye googlyEye;
  SpriteRenderer[] googlyEyeSpriteRenderers;
  LineRenderer connectionVisualizationRenderer;
  BoxCollider boxCollider;
  SphereCollider sphereCollider;

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
      decoration.sphereCollider = decoration.GetComponent<SphereCollider>();
    } else {
      decoration.spriteRenderer.sprite = DecorationUtils.DecorationTypeToImageResourceName(data.decorationType);
      // So the decoration shows up above all others when first being placed.
      decoration.spriteRenderer.sortingOrder = 1000000;
      decoration.boxCollider = decoration.GetComponent<BoxCollider>();
      decoration.boxCollider.size = DecorationUtils.HitboxSizeForDecoration(data.decorationType);
      decoration.boxCollider.center = DecorationUtils.HitboxCenterForDecoration(data.decorationType);
    }

    return decoration;
  }

  public void UpdateOrientation() {
    float scale = DecorationData.scale;
    transform.position = bone.transform.TransformPoint(new Vector3(DecorationData.offset.x, DecorationData.offset.y, Z_POSITION));
    transform.rotation = bone.transform.rotation * Quaternion.Euler(0f, 0f, DecorationData.rotation * Mathf.Rad2Deg);
    transform.localScale = new Vector3(scale, scale, scale);

    if (spriteRenderer != null) {
      spriteRenderer.flipX = DecorationData.flipX;
      spriteRenderer.flipY = DecorationData.flipY;
    }

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

  public int SetSpriteRendererOrder(int order) {
    if (DecorationData.decorationType != DecorationType.GooglyEye && spriteRenderer != null) {
      spriteRenderer.sortingOrder = order;
      return order + 1;
    }
    return order;
  }

  protected override void SetRendererMaterialForHighlight(Material mat, Material spriteMat, bool selected) {
    if (googlyEyeSpriteRenderers != null) {
      foreach (SpriteRenderer spriteRenderer in googlyEyeSpriteRenderers) {
        spriteRenderer.material = spriteMat;
      }
    }
    SetVisualizeConnection(selected);
  }

  public override void Delete() {
    base.Delete();
    Destroy(gameObject);
  }

  public override void PrepareForEvolution() {
    if (boxCollider != null) {
      // Let's not waste physics performance on decorations 
      boxCollider.enabled = false;
    }
    if (sphereCollider != null) {
      sphereCollider.enabled = false;
    }
  }
  
  public override int GetId() {
    return DecorationData.id;
  }
}