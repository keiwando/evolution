using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RaycastHover : MonoBehaviour {

	//private bool hovering = false;
	private HashSet<Collider> hoverColliders = new HashSet<Collider>();

	void Update () {

		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		var hits = Physics.RaycastAll(ray);

		if (hits.Length > 0) {

			foreach (var hit in hits) {
				SendOnHover(hit.collider);

				hoverColliders.Add(hit.collider);
			}

			var hitSet = new HashSet<Collider>(hits.Select(hit => hit.collider));

			foreach (var collider in hoverColliders.Except(hitSet)) {
				SendOnHoverExit(collider);
			}

			hoverColliders = hitSet;
		
		} else if (hoverColliders.Count > 0) {
			// Nothing hovered over -> exit on all
			ExitAll();
		}

		#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
		if (hoverColliders.Count > 0 && Input.touchCount == 0) {
			ExitAll();	
		}
		#endif
	}

	private void ExitAll() {
		foreach (var collider in hoverColliders) {
				SendOnHoverExit(collider);
			}

			hoverColliders.Clear();
	} 

	private void SendOnHover(Collider collider) {

		if (collider == null || collider.gameObject == null) return;

		collider.SendMessage("OnHover", SendMessageOptions.DontRequireReceiver);
	}

	private void SendOnHoverExit(Collider collider) {

		if (collider == null || collider.gameObject == null) return;

		collider.SendMessage("OnHoverExit", SendMessageOptions.DontRequireReceiver);
	}
}
