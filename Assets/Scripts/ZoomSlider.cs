using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ZoomSlider : MonoBehaviour {


	private float zoomLength = 15;
	private float minZoom;
	//private float maxZoom;

	public Slider slider;

	// Use this for initialization
	void Start () {

		float currentZoom = Camera.main.orthographicSize;
		minZoom = currentZoom - zoomLength / 2;
		//maxZoom = currentZoom + zoomLength + 2;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void updateZoom() {

		Camera.main.orthographicSize = zoomFromPercentage(1 - slider.value);
	}

	private float zoomFromPercentage(float percent) {

		return percent * zoomLength + minZoom;
	} 
}
