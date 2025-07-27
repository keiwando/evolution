﻿using System.Collections.Generic;

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
