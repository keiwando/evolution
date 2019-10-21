using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Keiwando.JSON;

public class NeuralNetworkSettingsManager : MonoBehaviour {

	private static readonly string INPUT_FIELD_PREFAB_NAME = "Prefabs/Layer Input Field";

	[SerializeField] private InputField numberOfLayersInput;

	// [SerializeField] private VisualNeuralNetwork visualNetwork;
	[SerializeField] private VisualNeuralNetworkRenderer networkRenderer;
	[SerializeField] private RectTransform leftEdge;
	[SerializeField] private RectTransform rightEdge;
	[SerializeField] private RawImage image;
	[SerializeField] private RenderTexture renderTextureAA;
	// For platforms that don't support AA textures (WebGL 1.0 Safari)
	[SerializeField] private RenderTexture renderTextureNoAA;
	private RenderTexture renderTexture;

	private Dictionary<int, InputField> intermediateInputs = new Dictionary<int, InputField>();

	void Start() {

		#if UNITY_WEBGL
		renderTexture = renderTextureNoAA;
		#else 
		renderTexture = renderTextureAA;
		#endif
		image.texture = renderTexture;
	
		numberOfLayersInput.onEndEdit.AddListener(delegate(string arg0) {
			NumberOfLayersChanged(arg0);
		});
		
		Refresh();
	}

	private void Refresh() {

		foreach (var input in intermediateInputs.Values) {
			Destroy(input.gameObject);
		}
		intermediateInputs.Clear();

		var settings = GetNetworkSettings();

		numberOfLayersInput.text = (settings.NumberOfIntermediateLayers + 2).ToString();

		var left = leftEdge.transform.localPosition.x;
		var right = rightEdge.transform.localPosition.x;
		var rectWidth = right - left;
		var spacing = rectWidth / (settings.NumberOfIntermediateLayers + 1);

		// Create an input field for every intermediate layer
		for (int i = 0; i < settings.NumberOfIntermediateLayers; i++) {

			var x = left + (i + 1) * spacing;
			var position = new Vector3(x, 0, transform.position.z);
			var input = CreateInputField(position);

			intermediateInputs.Add(i, input); 
			var index = i;

			input.onEndEdit.AddListener(delegate(string arg0) {

				LayerSizeInputChanged(index, arg0);
			});

			input.text = settings.NodesPerIntermediateLayer[i].ToString();
		}

		// visualNetwork.Refresh();
		networkRenderer.Render(settings, renderTexture);
	}

	public InputField CreateInputField(Vector3 position) {

		var input = ((GameObject)Instantiate(Resources.Load(INPUT_FIELD_PREFAB_NAME))).GetComponent<InputField>();

		//input.transform.SetParent(transform);
		input.transform.SetParent(transform, false);
		input.transform.localPosition = position;

		return input;
	}

	/// <summary>
	/// The input for the number of nodes for an intermediate changed.
	/// Index specifies the index of the hidden layer that was edited.
	/// </summary>
	public void LayerSizeInputChanged(int index, string value) {

		var settings = GetNetworkSettings();
		var num = Mathf.Clamp(int.Parse(value), 1, NeuralNetworkSettings.MAX_NODES_PER_LAYER);

		if (num == settings.NodesPerIntermediateLayer[index]) return;

		settings.NodesPerIntermediateLayer[index] = num;
		SaveNewSettings(settings);

		Refresh();
	} 

	/// <summary>
	/// The number of layers should be changed.
	/// </summary>
	/// <param name="value">Value.</param>
	public void NumberOfLayersChanged(string value) {

		int num = int.Parse(value);

		num = Mathf.Clamp(num, 3, NeuralNetworkSettings.MAX_LAYERS);

		numberOfLayersInput.text = num.ToString();

		var settings = GetNetworkSettings();

		var oldNumber = settings.NumberOfIntermediateLayers + 2;

		if (num != oldNumber) {
			// Number was changed
			var layerSizes = new List<int>(settings.NodesPerIntermediateLayer);

			if (num > oldNumber) {
				// Duplicate the last layer
				for ( int i = 0; i < num - oldNumber; i++)
					layerSizes.Add(layerSizes[layerSizes.Count - 1]);
			} else {
				for (int i = 0; i < oldNumber - num; i++)
					layerSizes.RemoveAt(layerSizes.Count - 1);	
			}

			SaveNewSettings(layerSizes.ToArray());
		}

		Refresh();
	}

	private void SaveNewSettings(int[] nodesPerIntermediateLayer) {

		var settings = new NeuralNetworkSettings();
		settings.NodesPerIntermediateLayer = nodesPerIntermediateLayer;

		SaveNewSettings(settings);
	}

	private void SaveNewSettings(NeuralNetworkSettings settings) {
		EditorStateManager.NetworkSettings = settings;
	}

	public void Reset() {
		
		var settings = NeuralNetworkSettings.Default;
		SaveNewSettings(settings);
		Refresh();
	}

	public static NeuralNetworkSettings GetNetworkSettings() {

		return EditorStateManager.NetworkSettings;
	}

}
