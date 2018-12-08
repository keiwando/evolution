using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TimeScaleSlider : MonoBehaviour {

	Evolution evolution;

	private float MIN_SCALE = 0f;
	private float MAX_SCALE = 5f;

	// Use this for initialization
	void Start () {
	
		evolution = GameObject.Find("Evolution").GetComponent<Evolution>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void setTimeScale(Slider slider) {
		evolution.TimeScale = slider.value * (MAX_SCALE - MIN_SCALE);
	}
}
