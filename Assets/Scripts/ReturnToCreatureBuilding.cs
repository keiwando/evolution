using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ReturnToCreatureBuilding : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		if(Input.GetKeyDown(KeyCode.Escape)) {
			SceneManager.LoadScene("CreatureBuildingScene");
		}
	}
}
