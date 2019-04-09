using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class InputUtils {

    /// <summary>
	/// Returns true if the mouse is positioned over a UI element.
	/// </summary>
	public static bool IsPointerOverUIObject(){

		PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
		eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

		List<RaycastResult> results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

		return results.Count > 0;
	}

	/// <summary>
	/// Returns true if the mouse/touch input ended on this frame.
	/// </summary>
	public static bool MouseUp() {
		return Input.GetMouseButtonUp(0) || 
			   (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended);
	}

	/// <summary>
	/// Returns true if the mouse/touch input began on an earlier
	/// frame and is still ongoing during this frame.
	/// </summary>
	public static bool MouseHeld() {
		return Input.GetMouseButton(0) ||
			   (Input.touchCount > 0 && (Input.GetTouch(0).phase != TouchPhase.Ended 
			   && Input.GetTouch(0).phase != TouchPhase.Ended));
	}
}