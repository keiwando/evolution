using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableOnMobile : MonoBehaviour {


	void Start () {
		if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer )
			gameObject.SetActive(false);	
	}
}