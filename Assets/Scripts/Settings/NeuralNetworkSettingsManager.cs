using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NeuralNetworkSettingsManager : MonoBehaviour {

	private static readonly string NETWORK_SETTINGS_KEY = "NEURAL NETWORK SETTINGS";

	private static readonly string INPUT_FIELD_PREFAB_NAME = "Prefabs/Layer Input Field";

	[SerializeField] private InputField numberOfLayersInput;

	[SerializeField] private VisualNeuralNetwork visualNetwork;

	private Dictionary<int, InputField> intermediateInputs = new Dictionary<int, InputField>();

	void Start() {
	
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

		numberOfLayersInput.text = (settings.numberOfIntermediateLayers + 2).ToString();

		// Make sure a visual network is connected
		if (visualNetwork == null) throw new System.NullReferenceException();

		visualNetwork.Setup();
		visualNetwork.networkSettings = settings;

		// Create an input field for every intermediate layer
		for (int i = 0; i < settings.numberOfIntermediateLayers; i++) {

			var position = new Vector3(visualNetwork.GetXPosForIntermediateLayer(i), 0, transform.position.z);
			var input = CreateInputField(position);

			intermediateInputs.Add(i, input); 
			var index = i;

			input.onEndEdit.AddListener(delegate(string arg0) {

				LayerSizeInputChanged(index, arg0);
			});

			input.text = settings.nodesPerIntermediateLayer[i].ToString();
		}

		visualNetwork.Refresh();
	}

	public InputField CreateInputField(Vector3 position) {

		var input = ((GameObject)Instantiate(Resources.Load(INPUT_FIELD_PREFAB_NAME))).GetComponent<InputField>();

		input.transform.SetParent(transform);
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

		if (num == settings.nodesPerIntermediateLayer[index]) return;

		settings.nodesPerIntermediateLayer[index] = num;
		SaveNewSettings(settings);

		Refresh();
	} 

	/// <summary>
	/// The number of layers should be changed.
	/// </summary>
	/// <param name="value">Value.</param>
	public void NumberOfLayersChanged(string value) {

		int num = int.Parse(value);

		num = Mathf.Clamp(num, 2, NeuralNetworkSettings.MAX_LAYERS);

		numberOfLayersInput.text = num.ToString();

		var settings = GetNetworkSettings();

		var oldNumber = settings.numberOfIntermediateLayers + 2;

		if (num != oldNumber) {
			// Number was changed
			var layerSizes = new List<int>(settings.nodesPerIntermediateLayer);

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
		settings.nodesPerIntermediateLayer = nodesPerIntermediateLayer;

		SaveNewSettings(settings);
	}

	private void SaveNewSettings(NeuralNetworkSettings settings) {
		PlayerPrefs.SetString(NETWORK_SETTINGS_KEY, settings.Encode());
	}

	public void Reset() {
		
		var settings = NeuralNetworkSettings.GetDefaultSettings();
		SaveNewSettings(settings);
		Refresh();
	}

	public static NeuralNetworkSettings GetNetworkSettings() {

		var settingsString = PlayerPrefs.GetString(NETWORK_SETTINGS_KEY, "");

		if (settingsString == "") {
			return NeuralNetworkSettings.GetDefaultSettings();
		}

		return NeuralNetworkSettings.Decode(settingsString);
	}

}
