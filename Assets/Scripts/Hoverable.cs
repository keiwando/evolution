using UnityEngine;
using System.Collections;

abstract public class Hoverable: MonoBehaviour {

	public bool shouldHighlight;

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

	public virtual void Start() {

		defaultShader = Shader.Find("Standard");
		highlightingShader = Shader.Find("Self-Illumin/Outlined Diffuse");

		highlightEmissionColor = new Color(0.7132353f, 0.5433174f, 0.2884408f, 1f);
		defaultEmissionColor = GetComponent<Renderer>().material.GetColor("_EmissionColor");
	}

	void OnMouseOver() {

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
	}
}
