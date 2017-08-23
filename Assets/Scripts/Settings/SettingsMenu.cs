using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour {

	public const string GRID_SIZE_KEY = "GRID_SIZE";
	public const string GRID_ENABLED_KEY = "GRID_ENABLED";

	public const string KEEP_BEST_CREATURE_KEY = "KEEP_BEST_CREATURE";

	private const float DEFAULT_GRID_SIZE = 2.0f;

	[SerializeField] private GameObject contentContainer;

	// Grid stuff
	[SerializeField] private Grid grid;
	[SerializeField] private Slider gridSizeSlider;
	[SerializeField] private Text gridSizeLabel;
	[SerializeField] private Toggle gridToggle;

	// Keep best creatures
	[SerializeField] private Toggle keepBestCreaturesToggle;

	// Use this for initialization
	void Start () {
		Setup();
	}

	private void Setup() {

		// Setup the grid stuff
		var gridSize = PlayerPrefs.GetFloat(GRID_SIZE_KEY, DEFAULT_GRID_SIZE);
		grid.Size = gridSize;
		gridSizeSlider.value = gridSize;
		UpdateGridSizeText(gridSize);

		var gridEnabled = PlayerPrefs.GetInt(GRID_ENABLED_KEY, 0) == 1;
		grid.gameObject.SetActive(gridEnabled);
		gridToggle.isOn = gridEnabled;

		var keepBestCreatures = PlayerPrefs.GetInt(KEEP_BEST_CREATURE_KEY, 0) == 1;
		keepBestCreaturesToggle.isOn = keepBestCreatures;

	}

	public void Show() {
		contentContainer.SetActive(true);
	}

	public void Hide() {
		contentContainer.SetActive(false);
	}

	public void GridToggled(bool val) {

		PlayerPrefs.SetInt(GRID_ENABLED_KEY, val ? 1 : 0);

		grid.gameObject.SetActive(val);
	}

	public void GridSizeChanged(float value) {

		UpdateGridSizeText(value);
		grid.Size = value;
		grid.VisualRefresh();

		PlayerPrefs.SetFloat(GRID_SIZE_KEY, value);
	}

	private void UpdateGridSizeText(float value) {
		gridSizeLabel.text = value.ToString("0.0");
	}

	public void KeepBestCreaturesToggled(bool value) {
		PlayerPrefs.SetInt(KEEP_BEST_CREATURE_KEY, value ? 1 : 0);
	}
}
