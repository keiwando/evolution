using UnityEngine;
using System.Collections;

abstract public class BodyComponent: Hoverable {

	public bool deleted;

	// Use this for initialization
	public override void Start () {
		base.Start();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	virtual public void delete(){
		deleted = true;
	}

	/** Prepares the component for the evolution simulation. */
	abstract public void prepareForEvolution();
}
