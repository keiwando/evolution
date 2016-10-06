using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Creature : MonoBehaviour {

	public List<Joint> joints;
	public List<Bone> bones;
	public List<Muscle> muscles;


	// Use this for initialization
	void Start () {

		joints = new List<Joint>();
		bones = new List<Bone>();
		muscles = new List<Muscle>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void setKinematic(bool enabled) {

		foreach (Joint joint in joints) {
			joint.GetComponent<Rigidbody>().isKinematic = enabled;
		}

		foreach (Bone bone in bones) {
			bone.GetComponent<Rigidbody>().isKinematic = enabled;
		}

		/*foreach (Muscle muscle in muscles) {
			muscle.GetComponent<Rigidbody>().isKinematic = enabled;
		}*/
	}

	public void prepareForEvolution() {

		foreach (Joint joint in joints) {
			joint.prepareForEvolution();
		}

		foreach (Bone bone in bones) {
			bone.prepareForEvolution();
		}

		foreach (Muscle muscle in muscles) {
			muscle.prepareForEvolution();
		}
	}
}
