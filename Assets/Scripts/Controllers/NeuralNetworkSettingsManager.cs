using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Keiwando.JSON;

public class NeuralNetworkSettingsUIManager : MonoBehaviour {

	private static readonly string INPUT_FIELD_PREFAB_NAME = "Prefabs/Layer Input Field";

	public GameObject neuralNetworkUIRootContainer;
	[SerializeField] private VisualNeuralNetworkRenderer networkRenderer;
	[SerializeField] private RectTransform leftEdge;
	[SerializeField] private RectTransform rightEdge;
	[SerializeField] private RawImage image;
	[SerializeField] private RenderTexture renderTextureAA;
	// For platforms that don't support AA textures (WebGL 1.0 Safari)
	[SerializeField] private RenderTexture renderTextureNoAA;
	private RenderTexture renderTexture;

	private Dictionary<int, InputField> intermediateInputs = new Dictionary<int, InputField>();

	private bool needsRefresh = false;

	public bool networkIsEditable = true;

	void Start() {

		#if UNITY_WEBGL
		renderTexture = renderTextureNoAA;
		#else 
		renderTexture = renderTextureAA;
		#endif
		image.texture = renderTexture;
		
		Refresh();
	}

	// This is not only needed for when the user dynamically changes the window size at runtime, but also
	// makes the UI positions work correctly when we launch the game with the settings menu active already.
  void OnRectTransformDimensionsChange() {
		needsRefresh = true;
  }

	void Update() {
		if (needsRefresh) {
			Refresh();
			needsRefresh = false;
		}
	}

  public void Refresh() {

		foreach (var input in intermediateInputs.Values) {
			Destroy(input.gameObject);
		}
		intermediateInputs.Clear();

		var settings = GetNetworkSettings();

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

		networkRenderer.Render(settings, renderTexture);
	}

	public InputField CreateInputField(Vector3 position) {

		var input = ((GameObject)Instantiate(Resources.Load(INPUT_FIELD_PREFAB_NAME))).GetComponent<InputField>();

		input.transform.SetParent(transform, false);
		input.transform.localPosition = position;
		input.interactable = networkIsEditable;

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

	private void SaveNewSettings(NeuralNetworkSettings settings) {
		if (!networkIsEditable) {
			return;
		}
		EditorStateManager.NetworkSettings = settings;
	}

	public static NeuralNetworkSettings GetNetworkSettings() {
		return EditorStateManager.NetworkSettings;
	}

}
