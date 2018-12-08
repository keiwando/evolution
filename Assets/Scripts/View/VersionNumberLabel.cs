using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VersionNumberLabel : MonoBehaviour {

	// Use this for initialization
	void Start () {

		var text = GetComponent<Text>();

		text.text =  string.Format("v {0}", Application.version.ToString());
	}
}
