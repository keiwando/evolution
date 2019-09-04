using UnityEngine;
using System.Collections;

abstract public class Hoverable: MonoBehaviour {

	private static Material highlightMaterial;

	public bool shouldHighlight {
		set {
			_shouldHighlight = value;
		}
		get { return _shouldHighlight; }
	}
	private bool _shouldHighlight;

	public bool hovering { get; private set; }

	private CursorMode cursorMode = CursorMode.Auto;
	private Vector2 hotSpot;

	private const float highlightAlpha = 0.5f;

	// Mobile input adjustments
	private const float hitBoxIncrease = 3f;
	private float defaultColliderRadius = 0.5f; 
	private bool isEnlarged = false;

	private Renderer _renderer;
	private Material normalMaterial;


	public virtual void Start() {

		highlightMaterial = Resources.Load("Materials/Selection Highlight") as Material;
		_renderer = GetComponent<Renderer>();
		normalMaterial = _renderer.sharedMaterial;
	}

	void OnDestroy() {
		Cursor.SetCursor(null, Vector2.zero, cursorMode);
	}

	public void EnableHighlight() {

		if (_renderer == null) return;
		_renderer.sharedMaterial = highlightMaterial;
	}

	public void DisableHighlight() {
		
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

		if (!isEnlarged) return;

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