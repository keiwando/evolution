using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropdownSortingFix : MonoBehaviour {

	private bool sortingFixed = false;
	private Canvas canvas;

	void Start() {
		canvas = GetComponent<Canvas>();
	}

	/*void Update() {
		return;
		if (!sortingFixed && canvas.sortingOrder > 10) {
			print("Sorting fixed");
			canvas.sortingOrder = 2;
			sortingFixed = true;
		}
	}*/
}
