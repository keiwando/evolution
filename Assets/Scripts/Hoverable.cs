using UnityEngine;
using System.Collections;

abstract public class Hoverable: MonoBehaviour {

	public bool shouldHighlight {
		set {
			_shouldHighlight = value;


			#if (UNITY_ANDROID || UNITY_IOS) 
			if (_shouldHighlight) {
				//ResetHitbox();
				EnlargeHitbox();
			}
			#endif
		}
		get { return _shouldHighlight; }
	}
	private bool _shouldHighlight;

	/** Specifies whether the mouse is hovering over the object. */
	public bool hovering { get; private set; }

	private Shader defaultShader;
	protected Shader highlightingShader;

	public Texture2D mouseHoverTexture;
	private CursorMode cursorMode = CursorMode.Auto;
	private Vector2 hotSpot;

	private const float highlightAlpha = 0.5f;

	private Color highlightEmissionColor;
	private Color defaultEmissionColor;

	// Mobile input adjustments
	private const float hitBoxIncrease = 2f; 
	private float defaultColliderRadius = 0.5f; 
	private bool isEnlarged = false;

	public virtual void Start() {

		defaultShader = Shader.Find("Standard");
		highlightingShader = Shader.Find("Self-Illumin/Outlined Diffuse");

		highlightEmissionColor = new Color(0.7132353f, 0.5433174f, 0.2884408f, 1f);
		defaultEmissionColor = GetComponent<Renderer>().material.GetColor("_EmissionColor");



		#if (UNITY_ANDROID || UNITY_IOS) 

		// increase the size of the hovering hitbox for easier creature building
		//EnlargeHitbox();
		#endif
	}

	/*void OnMouseOver() {

		print("Hovering");

		hovering = true;

		hotSpot = mouseHoverTexture == null ? Vector2.zero : new Vector2(mouseHoverTexture.width / 2, mouseHoverTexture.height / 2);

		if (shouldHighlight) {
			
			GetComponent<Renderer>().material.SetColor("_EmissionColor", highlightEmissionColor);
			Cursor.SetCursor(mouseHoverTexture, hotSpot, cursorMode);
		}
	}

	void OnMouseExit() {
		
		hovering = false;

		GetComponent<Renderer>().material.SetColor("_EmissionColor", defaultEmissionColor);
		Cursor.SetCursor(null, Vector2.zero, cursorMode);
	}*/

	void OnHover() {
		//print("OnHover");

		hovering = true;

		hotSpot = mouseHoverTexture == null ? Vector2.zero : new Vector2(mouseHoverTexture.width / 2, mouseHoverTexture.height / 2);

		if (shouldHighlight) {

			GetComponent<Renderer>().material.SetColor("_EmissionColor", highlightEmissionColor);
			Cursor.SetCursor(mouseHoverTexture, hotSpot, cursorMode);
		}
	}

	void OnHoverExit() {
		//print("OnHoverExit");

		hovering = false;

		GetComponent<Renderer>().material.SetColor("_EmissionColor", defaultEmissionColor);
		Cursor.SetCursor(null, Vector2.zero, cursorMode);
	}

	public void EnlargeHitbox() {

		if (isEnlarged) return;

		isEnlarged = true;

		var capsuleCollider = GetComponent<CapsuleCollider>();

		if ( capsuleCollider == null ) {

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

		//print("collider size reset");

		var capsuleCollider = GetComponent<CapsuleCollider>();

		if ( capsuleCollider == null ) {

			var sphereCollider = GetComponent<SphereCollider>();

			if (sphereCollider != null) {

				sphereCollider.radius = defaultColliderRadius;
			}
		} else {
			
			capsuleCollider.radius = defaultColliderRadius;
		}
	}


}
