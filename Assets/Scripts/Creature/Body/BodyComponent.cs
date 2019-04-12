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

	virtual public void Delete(){
		deleted = true;
	}

	/// <summary>
	/// Prepares the component for the evolution simulation.
	/// </summary>
	abstract public void PrepareForEvolution();

	/// <summary>
	/// Generates a strings that holds all the information needed to save and rebuild this BodyComponent.
	/// </summary>
	abstract public string GetSaveString();
}
