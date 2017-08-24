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

	public NeuralNetworkSettings networkSettings;

	private float leftEdge { get { return - Screen.width *  maxNetworkWidth / 2; } }
	private float rightEdge { get{ return - leftEdge; } }

	private List<Image> visualNodes = new List<Image>();
	private List<Image> nodeConnections = new List<Image>();

	private float minifyingScale = 1.0f;

	// Use this for initialization
	void Start () {

		networkSettings = new NeuralNetworkSettings();
		networkSettings.nodesPerIntermediateLayer = new int[]{ 3, 10, 5, 7, 6, 2, 14 };

		Refresh();
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
				//print(i);
				var numOfNodesInLayer = networkSettings.nodesPerIntermediateLayer[i];
				var top = -((numOfNodesInLayer - 1) * verticalNodeDistance * minifyingScale) / 2;
				var xPos = leftEdge + (i + 1) * (rightEdge - leftEdge) / (networkSettings.numberOfIntermediateLayers + 1);

				for (int j = 0; j < numOfNodesInLayer; j++) {

					var yPos = top + j * verticalNodeDistance * minifyingScale;
					
					var node = InstantiateNode();
					node.transform.localPosition = new Vector3(xPos, yPos, transform.position.z);

					nextLayer.Add(node);
				}
			}

			// Connect all of the current layer nodes with all of the next layer nodes.
			foreach (var node in currentLayer) {
				foreach (var nextNode in nextLayer) {

					var connection = InstantiateNodeConnection();
					PlaceNodeConnectionBetween(node.transform.position, nextNode.transform.position, connection);

					nodeConnections.Add(connection);
				}
			}

			visualNodes.AddRange(currentLayer);
			currentLayer.Clear();
		}

		visualNodes.AddRange(nextLayer);
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

		var rectTransform = connection.GetComponent<RectTransform>();
	
		rectTransform.sizeDelta = new Vector2(diff.magnitude, 1f);
		rectTransform.localScale = new Vector3(1, 5 * minifyingScale, 1);

		rectTransform.pivot = new Vector2(0.5f, 0.5f);
		rectTransform.position = (start + end) / 2;

		float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
		rectTransform.rotation = Quaternion.Euler(0,0, angle);
	}
}
