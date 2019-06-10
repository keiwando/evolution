using System;
using Keiwando.Evolution.UI;

namespace Keiwando.Evolution {

    public class SettingsManager: ISettingsViewControllerDelegate {

        private EditorSettings editorSettings {
            get => EditorStateManager.EditorSettings;
            set => EditorStateManager.EditorSettings = value;
        }
        private SimulationSettings simulationSettings {
            get => EditorStateManager.SimulationSettings;
            set {
                EditorStateManager.SimulationSettings = value;
                if (evolution != null) 
                    evolution.Settings = value;    
            }
        }

        private NeuralNetworkSettings networkSettings {
            get => EditorStateManager.NetworkSettings;
            set => EditorStateManager.NetworkSettings = value;
        }

        private Evolution evolution;
        public Grid grid;

        public SettingsManager(Evolution evolution = null) {
            this.evolution = evolution;
        }

        // MARK: - ISettingsViewControllerDelegate

        public bool IsGridActivated(GeneralSettingsView view) {
            return editorSettings.GridEnabled;
        }

        public float GetGridSize(GeneralSettingsView view) {
            return editorSettings.GridSize;
        }

        public float GetMinGridSize(GeneralSettingsView view) {
            return EditorSettings.MIN_GRID_SIZE;
        }

        public float GetMaxGridSize(GeneralSettingsView view) {
            return EditorSettings.MAX_GRID_SIZE;
        }

        public bool IsKeepBestCreatureEnabled(GeneralSettingsView view) {
            return simulationSettings.KeepBestCreatures;
        }

        public bool IsSimulateInBatchesEnabled(GeneralSettingsView view) {
            return simulationSettings.SimulateInBatches;
        }

        public int GetBatchSize(GeneralSettingsView view) {
            return simulationSettings.BatchSize;
        }

        public SelectionAlgorithm GetSelectionAlgorithm(GeneralSettingsView view) {
            return simulationSettings.SelectionAlgorithm;
        }

        public RecombinationAlgorithm GetRecombinationAlgorithm(GeneralSettingsView view) {
            return simulationSettings.RecombinationAlgorithm;
        }

        public MutationAlgorithm GetMutationAlgorithm(GeneralSettingsView view) {
            return simulationSettings.MutationAlgorithm;
        }

        public float GetMutationRate(GeneralSettingsView view) {
            return simulationSettings.MutationRate;
        }

        public int GetSimulationTime(PauseViewController view) {
            return simulationSettings.SimulationTime;
        }


        public void SimulationTimeChanged(PauseViewController view, int time) {
            var settings = simulationSettings;
            settings.SimulationTime = Math.Min(Math.Max(time, 1), 100000);
            simulationSettings = settings;
        }

        public void GridActivationToggled(GeneralSettingsView view, bool enabled) {
            var settings = editorSettings;
            settings.GridEnabled = enabled;
            editorSettings = settings;
            if (grid != null) grid.gameObject.SetActive(enabled);
        }

        public void GridSizeChanged(GeneralSettingsView view, float size) {
            var settings = editorSettings;
            settings.GridSize = size;
            editorSettings = settings;
            if (grid != null) {
                grid.Size = size;
                grid.VisualRefresh();
            }
        }

        public void KeepBestCreaturesToggled(GeneralSettingsView view, bool enabled) {
            var settings = simulationSettings;
            settings.KeepBestCreatures = enabled;
            simulationSettings = settings;
        }

        public void SimulateInBatchesToggled(GeneralSettingsView view, bool enabled) {
            var settings = simulationSettings;
            settings.SimulateInBatches = enabled;
            simulationSettings = settings;
        }

        public void BatchSizeChanged(GeneralSettingsView view, int batchSize) {
            var settings = simulationSettings;
            settings.BatchSize = batchSize;
            simulationSettings = settings;
        }

        public void SelectionAlgorithmChanged(GeneralSettingsView view, SelectionAlgorithm algorithm) {
            var settings = simulationSettings;
            settings.SelectionAlgorithm = algorithm;
            simulationSettings = settings;
        }

        public void RecombinationAlgorithmChanged(GeneralSettingsView view, RecombinationAlgorithm algorithm) {
            var settings = simulationSettings;
            settings.RecombinationAlgorithm = algorithm;
            simulationSettings = settings;
        }

        public void MutationAlgorithmChanged(GeneralSettingsView view, MutationAlgorithm algorithm) {
            var settings = simulationSettings;
            settings.MutationAlgorithm = algorithm;
            simulationSettings = settings;
        }

        public void MutationRateChanged(GeneralSettingsView view, float mutationRate) {
            var settings = simulationSettings;
            settings.MutationRate = Math.Min(Math.Max(mutationRate, 0), 1);
            simulationSettings = settings;
        }

        public void ResetButtonPressed(NetworkSettingsView view) {
            networkSettings = NeuralNetworkSettings.Default;
        }
    }
}

