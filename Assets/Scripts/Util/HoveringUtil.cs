using System.Collections.Generic;
using UnityEngine;

public static class HoveringUtil {

    /// <summary>
	/// Sets the shouldHighlight property of all list entries in the 
	/// specified list to the value specified by shouldHighlight.
	/// </summary>
	public static void SetShouldHighlight<T>(List<T> hoverables, bool shouldHighlight) where T: Hoverable {

		foreach (var obj in hoverables) {
			obj.shouldHighlight = shouldHighlight;
		}
	} 

    /// <summary>
	/// Returns the Object that the mouse is currently hovering over or null 
	/// if there is no such object in the given list. The list contains scripts
	/// that are attached to gameobjects which have a HoverableScript attached.
	/// </summary>
	public static T GetHoveringObject<T>(List<T> objects) where T: Hoverable {

		foreach (T obj in objects) {
			if (obj.hovering) {
				return obj;
			}
		}

		return null;
	}

    /// <summary>
	/// Resets the hoverable colliders on the list items. 
	/// </summary>
	public static void ResetHoverableColliders<T>(List<T> hoverables) where T: Hoverable {

		foreach (Hoverable hov in hoverables) {
			hov.ResetHitbox();
		}
	}

	/// <summary>
	/// Enlarges the hoverable colliders on the list items. 
	/// </summary>
	public static void EnlargeHoverableColliders<T>(List<T> hoverables) where T: Hoverable {

		foreach (Hoverable hov in hoverables) {
			hov.EnlargeHitbox();
		}
	}
}