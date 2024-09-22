using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Keiwando.Evolution.UI {

	public class EvolutionPauseMenu : MonoBehaviour {

		private Evolution evolution;

		// Keep best creatures
		[SerializeField] 
		private Toggle keepBestCreaturesToggle;

		// Batch simulation
		[SerializeField] 
		private InputField batchSizeInput;
		[SerializeField] 
		private Toggle batchSizeToggle;

		// Simulation Time
		[SerializeField] 
		private InputField simulationTimeInput;

		// Mutation rate
		[SerializeField] 
		private InputField mutationRateInput;

		// Use this for initialization
		void Start () {

			evolution = FindAnyObjectByType<Evolution>();

			Setup();
		}

		private void Setup() {

			SetupInputCallbacks();

			// Evolution settings
			var settings = evolution.Settings;

			keepBestCreaturesToggle.isOn = settings.KeepBestCreatures;

			BatchSizeToggled(settings.SimulateInBatches);
			batchSizeToggle.isOn = settings.SimulateInBatches;

			batchSizeInput.text = settings.BatchSize.ToString();
			simulationTimeInput.text = settings.SimulationTime.ToString();
			mutationRateInput.text = settings.MutationRate.ToString();
		}

		private void SetupInputCallbacks() {

			simulationTimeInput.onEndEdit.AddListener(delegate {
				SimulationTimeChanged();
			});

			batchSizeInput.onEndEdit.AddListener(delegate {
				BatchSizeChanged();
			});

			mutationRateInput.onEndEdit.AddListener(delegate {
				MutationRateChanged();
			});

			keepBestCreaturesToggle.onValueChanged.AddListener(delegate(bool arg0) {
				KeepBestCreaturesToggled(arg0);	
			});

			batchSizeToggle.onValueChanged.AddListener(delegate(bool arg0) {
				BatchSizeToggled(arg0);	
			});
		}

		public void Pause() {
			this.gameObject.SetActive(true);

			Time.timeScale = 0;
		}

		public void Continue() {
			this.gameObject.SetActive(false);
			Time.timeScale = 1f;
		}

		public void KeepBestCreaturesToggled(bool value) {
			
			var settings = evolution.Settings;
			settings.KeepBestCreatures = value;
			evolution.Settings = settings;
		}

		private void SimulationTimeChanged() {
			// Make sure the time is at least 1
			var time = Mathf.Clamp(Int32.Parse(simulationTimeInput.text), 1, 100000);
			simulationTimeInput.text = time.ToString();

			/*var settings = LoadSimulationSettings();
			settings.simulationTime = time;
			SaveSimulationSettings(settings);*/
			var settings = evolution.Settings;
			settings.SimulationTime = time;
			evolution.Settings = settings;
		}

		private void MutationRateChanged() {
			// Clamp between 1 and 100 %
			var rate = Mathf.Clamp(int.Parse(mutationRateInput.text), 1, 100);
			mutationRateInput.text = rate.ToString();

			/*var settings = LoadSimulationSettings();
			settings.mutationRate = rate;
			SaveSimulationSettings(settings);*/

			var settings = evolution.Settings;
			settings.MutationRate = rate;
			evolution.Settings = settings;
		}

		private void BatchSizeChanged() {
			// Make sure the size is between 1 and the population size
			var batchSize = ClampBatchSize(Int32.Parse(batchSizeInput.text));
			batchSizeInput.text = batchSize.ToString();

			/*var settings = LoadSimulationSettings();
			settings.batchSize = batchSize;
			SaveSimulationSettings(settings);*/

			var settings = evolution.Settings;
			settings.BatchSize = batchSize;
			evolution.Settings = settings;
		}

		private int ClampBatchSize(int size) {

			//var populationSize = Mathf.Clamp(Int32.Parse(populationSizeInput.text), 2, 10000000);
			var populationSize = evolution.Settings.PopulationSize;

			return Mathf.Clamp(size, 1, populationSize);
		}

		public void BatchSizeToggled(bool val) {

			//PlayerPrefs.SetInt(BATCH_SIMULATION_ENABLED_KEY, val ? 1 : 0);
			batchSizeInput.gameObject.SetActive(val);

			/*var settings = LoadSimulationSettings();
			settings.simulateInBatches = val;
			SaveSimulationSettings(settings);*/

			var settings = evolution.Settings;
			settings.SimulateInBatches = val;
			evolution.Settings = settings;
		}
	}
}
