using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour {

	private const float gridAreaSize = 70; 

	public float Size {
		get { return gridSize; }
		set { gridSize = value; }
	}
	private float gridSize = 2f;

	private float left { get {return -gridAreaSize / 2;} }
	private float right { get {return gridAreaSize / 2;} }
	private float top { get {return gridAreaSize * (2f / 3);} }
	private float bottom { get {return -gridAreaSize * (1f / 3);} }

	private LineRenderer lineRenderer;

	// Use this for initialization
	void Start () {
		SetupGrid();
	}

	/// <summary>
	/// Returns the closest point on the grid to the given point.
	/// </summary>
	public Vector3 ClosestPointOnGrid(Vector3 point) {

		return Vec(ClosestVerticalLine(point.x), ClosestHorizontalLine(point.y));
	}

	private float ClosestHorizontalLine(float y) {

		var k = Mathf.Floor((y - bottom) / gridSize);
		var bottomY = k * gridSize + bottom;
		var topY = bottomY + gridSize;

		return y - bottomY > topY - y ? topY : bottomY;
	} 

	private float ClosestVerticalLine(float x) {

		// k * gridSize + left < x < (k+1) * gridSize + left

		var k = Mathf.Floor((x - left) / gridSize);
		var leftX = k * gridSize + left;
		var rightX = leftX + gridSize;

		return x - leftX > rightX - x ? rightX : leftX;
	}

	private void SetupGrid() {

		// Create a bunch of lines
		lineRenderer = gameObject.GetComponent<LineRenderer>();
		lineRenderer.startWidth = 0.11f;
		lineRenderer.endWidth = 0.11f;

		var color = new Color(0,0,0,0.4f);
		lineRenderer.startColor = color;
		lineRenderer.endColor = color;

		lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		lineRenderer.receiveShadows = false;

		var vertices = CalculateVertices();

		lineRenderer.positionCount = vertices.Length;
		lineRenderer.SetPositions(vertices);

	}

	private Vector3[] CalculateVertices() {

		var vertices = new List<Vector3>();

		// Add all the vertical sections
		for (int i = 0; i < (gridAreaSize / (2 * gridSize)); i++) {

			vertices.AddRange(VerticalSection(left + i * 2 * gridSize));
		}

		vertices.Add(Vec(left, top));

		// Add all the horizontal sections
		for (int i = 0; i < (gridAreaSize / (2 * gridSize)); i++) {

			vertices.AddRange(HorizontalSection(top - i * 2 * gridSize));
		}

		return vertices.ToArray();
	}

	/// <summary>
	/// Returns vertices in order that form two horizontal lines of the grid.
	/// The section follows (left, y) -> (right, y) -> (right, y - gridSize) -> (left, y - gridSize)
	/// </summary>
	private Vector3[] HorizontalSection(float y) {
		
		return new Vector3[] { 
			Vec(left, y),
			Vec(right, y),
			Vec(right, y - gridSize),
			Vec(left, y - gridSize)
		};
	}

	/// <summary>
	/// Returns vertices in order that form two vertical lines of the grid.
	/// The section follows (x, top) -> (x, bottom) -> (x + gridSize, bottom) -> (x + gridSize, top) 
	/// </summary>
	private Vector3[] VerticalSection(float x) {
		
		return new Vector3[] { 
			Vec(x, top),
			Vec(x, bottom),
			Vec(x + gridSize, bottom),
			Vec(x + gridSize, top)
		};
	}

	private Vector3 Vec(float x, float y) {
		return new Vector3(x, y, 0);
	}
}
