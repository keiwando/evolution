using UnityEngine;
using System.Collections;

abstract public class Hoverable: MonoBehaviour {

	public bool shouldHighlight;

	/** Specifies whether the mouse is hovering over the object. */
	public bool hovering { get; private set; }

	private Shader defaultShader;
	private Shader highlightingShader;

	public virtual void Start() {

		defaultShader = Shader.Find("Diffuse");
		highlightingShader = Shader.Find("Self-Illumin/Outlined Diffuse");
	}

	void OnMouseOver() {

		hovering = true;

		if (shouldHighlight) {
			GetComponent<Renderer>().material.shader = highlightingShader;
		}
	}

	void OnMouseExit() {

		hovering = false;

		GetComponent<Renderer>().material.shader = defaultShader;
	}
}
