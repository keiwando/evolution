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

	public virtual void Start() {

		defaultShader = Shader.Find("Standard");
		highlightingShader = Shader.Find("Self-Illumin/Outlined Diffuse");

	}

	void OnMouseOver() {

		hovering = true;

		hotSpot = mouseHoverTexture == null ? Vector2.zero : new Vector2(mouseHoverTexture.width / 2, mouseHoverTexture.height / 2);

		if (shouldHighlight) {
			GetComponent<Renderer>().material.shader = highlightingShader;
			Cursor.SetCursor(mouseHoverTexture, hotSpot, cursorMode);
		}
	}

	void OnMouseExit() {

		hovering = false;

		GetComponent<Renderer>().material.shader = defaultShader;
		Cursor.SetCursor(null, Vector2.zero, cursorMode);
	}
}
