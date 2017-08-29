using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MuscleJoint : MonoBehaviour { //: Hoverable

	public int ID;

	public Vector3 position { 
		get {
			return transform.position;
		} 
	}

	private List<Muscle> connectedMuscles = new List<Muscle>();

	// Use this for initialization
	void Start () {
		
	}

	public void Connect(Muscle muscle) {

		connectedMuscles.Add(muscle);
	}

	public void Disconnect(Muscle muscle) {
		
		connectedMuscles.Remove(muscle);
	}

	public void deleteAllConnected() {

		var connected = new List<Muscle>(connectedMuscles);

		foreach (Muscle muscle in connected) {
			if (muscle != null) {

				muscle.Delete();
			}
		} 

		connectedMuscles.Clear();
	}
}
