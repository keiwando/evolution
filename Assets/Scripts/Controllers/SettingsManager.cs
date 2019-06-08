using Keiwando.Evolution.UI;

namespace Keiwando.Evolution {

    public class SettingsManager: ISettingsViewControllerDelegate {

        private EditorSettings editorSettings => EditorStateManager.LoadEditorSettings();
        private SimulationSettings simulationSettings => EditorStateManager.LoadSimulationSettings();
        private NeuralNetworkSettings networkSettings => EditorStateManager.LoadNetworkSettings();

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
            // TODO: Load from SimulationSettings
            return SelectionAlgorithm.FitnessProportional;
        }

        public RecombinationAlgorithm GetRecombinationAlgorithm(GeneralSettingsView view) {
            // TODO: Load from SimulationSettings
            return RecombinationAlgorithm.MultiPointCrossover;
        }

        public MutationAlgorithm GetMutationAlgorithm(GeneralSettingsView view) {
            // TODO: Load from SimulationSettings
            return MutationAlgorithm.ChunkFlip;
        }

        public float GetMutationRate(GeneralSettingsView view) {
            return simulationSettings.MutationRate / 100f;
        }


        public void GridActivationToggled(GeneralSettingsView view, bool enabled) {
            editorSettings.
        }

        public void GridSizeChanged(GeneralSettingsView view, float size) {

        }

        public void KeepBestCreaturesToggled(GeneralSettingsView view, bool enabled) {

        }

        public void SimulateInBatchesToggled(GeneralSettingsView view, bool enabled) {

        }

        public void BatchSizeChanged(GeneralSettingsView view, int batchSize) {

        }

        public void SelectionAlgorithmChanged(GeneralSettingsView view, SelectionAlgorithm algorithm) {

        }

        public void RecombinationAlgorithmChanged(GeneralSettingsView view, RecombinationAlgorithm algorithm) {

        }

        public void MutationAlgorithmChanged(GeneralSettingsView view, MutationAlgorithm algorithm) {

        }

        public void MutationRateChanged(GeneralSettingsView view, float mutationRate) {

        }
    }
}

