using UnityEngine;
using System.Collections;

abstract public class Hoverable: MonoBehaviour {

	private static Material highlightMaterial;

	public bool hovering { get; private set; }

	private Renderer _renderer;
	private Material normalMaterial;

	public virtual void Start() {

		if (highlightMaterial == null)
			highlightMaterial = Resources.Load("Materials/Selection Highlight") as Material;
		_renderer = GetComponent<Renderer>();
		normalMaterial = _renderer.sharedMaterial;
	}

	void OnDestroy() {
		Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
	}

	public void EnableHighlight() {

		if (_renderer == null) return;
		_renderer.sharedMaterial = highlightMaterial;
	}

	public void DisableHighlight() {
		
		if (_renderer == null) return;
		_renderer.sharedMaterial = normalMaterial;
	}
}