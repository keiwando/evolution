using System.Collections.Generic;
using UnityEngine;

public class RaycastHover : MonoBehaviour {

	private HashSet<Collider> hoverColliders = new HashSet<Collider>();
	private HashSet<Collider> _newHoverColliders = new HashSet<Collider>();
	private RaycastHit[] _hits = new RaycastHit[100];

	void Update () {

		bool canHover = true;
		#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
		canHover = Input.touchCount > 0;
		#endif

		if (canHover) {

			Ray ray = Camera.main.ScreenPointToRay(InputUtils.GetMousePosition());
			int hitsLength = Physics.RaycastNonAlloc(ray, _hits);

			if (hitsLength > 0) {

				_newHoverColliders.Clear();
				for (int i = 0; i < hitsLength; i++) {
					var hit = _hits[i];
					SendOnHover(hit.collider);

					_newHoverColliders.Add(hit.collider);
				}

				foreach (var collider in hoverColliders) {
					if (!_newHoverColliders.Contains(collider)) {
						SendOnHoverExit(collider);
					}
				}

				var tmp = hoverColliders;
				hoverColliders = _newHoverColliders;
				_newHoverColliders = tmp;
			
			} else if (hoverColliders.Count > 0) {
				// Nothing hovered over -> exit on all
				ExitAll();
			}
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
