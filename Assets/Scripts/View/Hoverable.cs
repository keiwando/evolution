using UnityEngine;
using System.Collections;

abstract public class Hoverable: MonoBehaviour {

	private static Material highlightMaterial;

	public bool shouldHighlight {
		set {
			_shouldHighlight = value;

			#if (UNITY_ANDROID || UNITY_IOS) 
			if (_shouldHighlight) {
				EnlargeHitbox();
			}
			#endif
		}
		get { return _shouldHighlight; }
	}
	private bool _shouldHighlight;

	/** Specifies whether the mouse is hovering over the object. */
	public bool hovering { get; private set; }

	public Texture2D mouseHoverTexture;
	private CursorMode cursorMode = CursorMode.Auto;
	private Vector2 hotSpot;

	private const float highlightAlpha = 0.5f;

	// private Color highlightEmissionColor;
	// private Color defaultEmissionColor;

	// Mobile input adjustments
	private const float hitBoxIncrease = 2f;
	private float defaultColliderRadius = 0.5f; 
	private bool isEnlarged = false;

	private Renderer _renderer;
	private Material normalMaterial;


	public virtual void Start() {

		highlightMaterial = Resources.Load("Materials/Selection Highlight") as Material;
		_renderer = GetComponent<Renderer>();
		normalMaterial = _renderer.sharedMaterial;
		// highlightEmissionColor = new Color(0.7132353f, 0.5433174f, 0.2884408f, 1f);
		// defaultEmissionColor = GetComponent<Renderer>().sharedMaterial.GetColor("_EmissionColor");		
	}

	void OnHover() {

		hovering = true;

		// TODO: Replace this with changing out the material completely 
		// instead of altering material properties

		hotSpot = mouseHoverTexture == null ? Vector2.zero : new Vector2(mouseHoverTexture.width / 2, mouseHoverTexture.height / 2);

		if (shouldHighlight) {

			Highlight();
			Cursor.SetCursor(mouseHoverTexture, hotSpot, cursorMode);
		}
	}

	void OnHoverExit() {

		hovering = false;

		if (shouldHighlight)

		DisableHighlight();
		Cursor.SetCursor(null, Vector2.zero, cursorMode);
	}

	void OnDestroy() {
		Cursor.SetCursor(null, Vector2.zero, cursorMode);
	}

	public void Highlight() {
		// GetComponent<Renderer>().material.SetColor("_EmissionColor", highlightEmissionColor);
		if (_renderer == null) return;
		_renderer.sharedMaterial = highlightMaterial;
	}

	public void DisableHighlight() {
		// GetComponent<Renderer>().material.SetColor("_EmissionColor", defaultEmissionColor);
		if (_renderer == null) return;
		_renderer.sharedMaterial = normalMaterial;
	}

	public void EnlargeHitbox() {

		if (isEnlarged) return;

		isEnlarged = true;

		var capsuleCollider = GetComponent<CapsuleCollider>();

		if (capsuleCollider == null) {

			var sphereCollider = GetComponent<SphereCollider>();

			if (sphereCollider != null) {
				defaultColliderRadius = sphereCollider.radius;
				sphereCollider.radius *= hitBoxIncrease;
			}
		} else {
			defaultColliderRadius = capsuleCollider.radius;
			capsuleCollider.radius *= hitBoxIncrease;
		}

	}

	public void ResetHitbox() {

		isEnlarged = false;

		var capsuleCollider = GetComponent<CapsuleCollider>();

		if (capsuleCollider == null) {

			var sphereCollider = GetComponent<SphereCollider>();

			if (sphereCollider != null) {
				sphereCollider.radius = defaultColliderRadius;
			}
		} else {
			capsuleCollider.radius = defaultColliderRadius;
		}
	}
}