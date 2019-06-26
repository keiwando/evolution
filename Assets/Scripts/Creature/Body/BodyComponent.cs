using System.Collections.Generic;

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

	/// <summary>
	/// Removes the already destroyed object that are still left in the list.
	/// This operation doesn't change the original list.
	/// </summary>
	/// <param name="objects">A list of BodyComponents</param>
	/// <typeparam name="T">A BodyComponent subtype.</typeparam>
	/// <returns>A list without the already destroyed objects of the input list.</returns>
	public static List<T> RemoveDeletedObjects<T>(List<T> objects) where T: BodyComponent {

		List<T> removed = new List<T>(objects);
		foreach (T obj in objects) {
			if (obj == null || obj.Equals(null) || obj.gameObject == null 
				|| obj.gameObject.Equals(null) || obj.deleted) {
	
				removed.Remove(obj);
			}
		}
		return removed;
	}
}
