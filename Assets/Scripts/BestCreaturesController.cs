using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BestCreaturesController : MonoBehaviour {

	/// <summary>
	/// The camera that follows the creatures in the main evolution "scene".
	/// </summary>
	public CameraFollowScript MainCamera;
	/// <summary>
	/// The camera used to follow the best creature of the selected generation.
	/// </summary>
	public CameraFollowScript BCCamera;

	public Canvas EvolutionCanvas;
	public Canvas BCCanvas;

	public Transform floorHeight;
	public Vector3 dropHeight;

	/// <summary>
	/// The list of best creature brains (as chromosome strings). The index + 1 = generation Number.
	/// </summary>
	private List<string> BestCreatures;

	// Use this for initialization
	void Start () {
		BestCreatures = new List<string>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	/// <summary>
	/// Shows the best creatures "scene".
	/// </summary>
	public void ShowBestCreatures() {

		EvolutionCanvas.gameObject.SetActive(false);
		BCCanvas.gameObject.SetActive(true);

		MainCamera.SwitchToMiniViewport();
		BCCamera.SwitchToFullscreen();
	}

	/// <summary>
	/// Shows the main evolution / simulation scene.
	/// </summary>
	public void ShowEvolution() {

		EvolutionCanvas.gameObject.SetActive(true);
		BCCanvas.gameObject.SetActive(false);

		MainCamera.SwitchToFullscreen();
		BCCamera.SwitchToMiniViewport();
	}

	public void AddBestCreature(int generation, string chromosome) {
		if (generation <= 0) throw new UnityException();

		BestCreatures.Add(chromosome);
	}


}
