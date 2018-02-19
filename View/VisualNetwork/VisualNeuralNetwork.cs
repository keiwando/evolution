using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class VisualNeuralNetwork : MonoBehaviour {

	private const string NODE_PREFAB_NAME = "Prefabs/Node";
	private const string NODE_CONNECTION_PREFAB_NAME = "Prefabs/Brain Node Connection";

	// As a multiple of the screen width 
	public float maxNetworkWidth;

	public float verticalNodeDistance;

	/// <summary>
	/// The maximum number of visual connections between two consecutive layers.
	/// Needed for performance reasons. Too many connections are not needed anyway
	/// and will just cause the whole app to crash. 
	/// </summary>
	[SerializeField] private int maxNumberOfConnections;

	public NeuralNetworkSettings networkSettings {
		set {
			_settings = value;
			Refresh();
		}
		get {
			return _settings;
		}
	}
	private NeuralNetworkSettings _settings;

	private float horizontalNodeDistance {
		get {
			return (rightEdge - leftEdge) / (networkSettings.numberOfIntermediateLayers + 1);
		}
	}

	private float canvasWidth = 1.0f;
	//private float canvasHeight = 1.0f;

	private float leftEdge { get { return - canvasWidth *  maxNetworkWidth / 2; } }
	private float rightEdge { get { return - leftEdge; } }

	private List<Image> visualNodes = new List<Image>();
	private List<Image> nodeConnections = new List<Image>();

	private float minifyingScale = 1.0f;

	// Use this for initialization
	void Start () {

		Setup();
		Refresh();
	}

	public void Setup() {
		var canvas = GameObject.FindGameObjectWithTag("SettingsCanvas");
		var canvasRect = canvas.GetComponent<RectTransform>().rect;
		canvasWidth = canvasRect.width;
		//canvasHeight = canvasRect.height;

		_settings = NeuralNetworkSettingsManager.GetNetworkSettings();
	}

	public void Refresh() {

		var maxNodesPerLayer = networkSettings.nodesPerIntermediateLayer.Max();
		minifyingScale = Mathf.Min(1.0f, 10f / maxNodesPerLayer);

		DeleteCurrentNet();

		SetupVisualNet();
	}

	private void SetupVisualNet() {

		var currentLayer = new List<Image>();
		var nextLayer = new List<Image>();

		for (int i = 0; i < networkSettings.numberOfIntermediateLayers + 1; i++) {

			currentLayer.Clear();
			currentLayer.AddRange(nextLayer);
			nextLayer.Clear();

			if (i == 0) {
				var input = InstantiateNode();
				input.transform.localPosition = new Vector3(leftEdge, 0, transform.position.z);
				currentLayer.Add(input);
			} 

			// Create the next layer nodes
			if (i == networkSettings.numberOfIntermediateLayers) {
				var output = InstantiateNode();
				output.transform.localPosition = new Vector3(rightEdge, 0, transform.position.z);

				nextLayer.Add(output);
			
			} else {
				// The next layer is an intermediate layer
				var numOfNodesInLayer = networkSettings.nodesPerIntermediateLayer[i];
				var top = -((numOfNodesInLayer - 1) * verticalNodeDistance * minifyingScale) / 2;
				var xPos = GetXPosForIntermediateLayer(i);

				for (int j = 0; j < numOfNodesInLayer; j++) {

					var yPos = top + j * verticalNodeDistance * minifyingScale;
					
					var node = InstantiateNode();
					node.transform.localPosition = new Vector3(xPos, yPos, transform.position.z);

					nextLayer.Add(node);
				}
			}

			// Connect the current layer nodes with the next layer nodes.
			var outConnectionsPerNode = Mathf.Min(maxNumberOfConnections / currentLayer.Count, nextLayer.Count);
			var nextIndices = Enumerable.Range(0, nextLayer.Count);
			var rand = new System.Random();

			foreach (var node in currentLayer) {

				var indices = nextIndices.OrderBy(x => rand.Next()).Take(outConnectionsPerNode);

				foreach (var ind in indices) {

					var nextNode = nextLayer[ind];
					var connection = InstantiateNodeConnection();
					PlaceNodeConnectionBetween(node.transform.position, nextNode.transform.position, connection);

					nodeConnections.Add(connection);
				}
			}

			visualNodes.AddRange(currentLayer);
			currentLayer.Clear();
		}

		visualNodes.AddRange(nextLayer);

		// Move all of the connection up in the hierarchy so that they are drawn first
		foreach (var connection in nodeConnections) {
			connection.transform.SetSiblingIndex(0);
		}
	}

	private void DeleteCurrentNet() {
		foreach (var node in visualNodes) {
			Destroy(node.gameObject);
		} 

		foreach (var connection in nodeConnections) {
			Destroy(connection.gameObject);
		}

		visualNodes.Clear();
		nodeConnections.Clear();
	}

	private Image InstantiateNode() {

		var img = ((GameObject) Instantiate(Resources.Load(NODE_PREFAB_NAME))).GetComponent<Image>();
		img.transform.SetParent(transform);

		img.transform.localScale = new Vector3(minifyingScale, minifyingScale, 1.0f);

		return img;
	}

	private Image InstantiateNodeConnection() {
		var img = ((GameObject) Instantiate(Resources.Load(NODE_CONNECTION_PREFAB_NAME))).GetComponent<Image>();
		img.transform.SetParent(transform);

		return img;
	}

	private void PlaceNodeConnectionBetween(Vector3 start, Vector3 end, Image connection) {

		Vector3 diff = end - start;

		var lengthMultiply = 1.0f * canvasWidth / Screen.width;

		var rectTransform = connection.GetComponent<RectTransform>();
	
		rectTransform.sizeDelta = new Vector2(diff.magnitude * lengthMultiply, 1f);
		rectTransform.localScale = new Vector3(1, 5 * minifyingScale, 1);

		rectTransform.pivot = new Vector2(0.5f, 0.5f);
		rectTransform.position = (start + end) / 2;

		float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
		rectTransform.rotation = Quaternion.Euler(0,0, angle);
	}

	public float GetXPosForIntermediateLayer(int index) {
		
		return leftEdge + (index + 1) * horizontalNodeDistance;
	}
}
