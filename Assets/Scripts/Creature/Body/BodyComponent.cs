using System.Collections.Generic;

public enum BodyComponentType {
	Joint,
	Bone,
	Muscle,
	Decoration
}

abstract public class BodyComponent: Hoverable {

	public bool deleted { get; private set; }

	public override void Start () {
		base.Start();
	}

	virtual public void Delete(){
		deleted = true;
	}

	/// <summary>
	/// Prepares the component for the evolution simulation.
	/// </summary>
	abstract public void PrepareForEvolution();

	abstract public BodyComponentType GetBodyComponentType();

	/// <summary>
	/// Returns the id of this component.
	/// </summary>
	/// <returns></returns>
	abstract public int GetId();

	/// <summary>
	/// Removes the already destroyed object that are still left in the list.
	/// This operation doesn't change the original list.
	/// </summary>
	/// <param name="objects">A list of BodyComponents</param>
	/// <typeparam name="T">A BodyComponent subtype.</typeparam>
	public static void RemoveDeletedObjects<T>(List<T> objects) where T: BodyComponent {

		for (int i = objects.Count - 1; i >= 0; i--) {
			T obj = objects[i];
			if (obj == null || obj.Equals(null) || obj.gameObject == null 
				|| obj.gameObject.Equals(null) || obj.deleted) {
				
				objects.RemoveAt(i);
			}
		}
	}
}
