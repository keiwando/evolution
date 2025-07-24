using UnityEngine;
using System.Collections;

abstract public class Hoverable: MonoBehaviour {

	public static Material highlightMaterial;
	public static Material spriteHighlightMaterial;

	public bool hovering { get; private set; }

	private Renderer _renderer;
	private Material normalMaterial;
	private bool isSpriteRenderer;

	public virtual void Start() {

		if (highlightMaterial == null) {
			highlightMaterial = Resources.Load("Materials/Selection Highlight") as Material;
		}
		if (spriteHighlightMaterial == null) {
			spriteHighlightMaterial = Resources.Load("Materials/Selection Highlight Sprite") as Material;
		}
		_renderer = GetComponent<Renderer>();
		normalMaterial = _renderer.sharedMaterial;
		isSpriteRenderer = _renderer is SpriteRenderer;
	}

	void OnDestroy() {
		Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
	}

	public void EnableHighlight() {

		SetRendererMaterialForHighlight(highlightMaterial, spriteHighlightMaterial, true);
		if (_renderer == null) return;
		_renderer.sharedMaterial = isSpriteRenderer ? spriteHighlightMaterial : highlightMaterial;
	}

	public void DisableHighlight() {
		
		SetRendererMaterialForHighlight(normalMaterial, normalMaterial, false);
		if (_renderer == null) return;
		_renderer.sharedMaterial = normalMaterial;
	}

	protected virtual void SetRendererMaterialForHighlight(Material mat, Material spriteMaterial, bool selected) {}
}