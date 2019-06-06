using UnityEngine;
using UnityEngine.UI;

namespace Keiwando.Evolution.UI {

    public interface IGeneralSettingsViewDelegate {

        bool IsGridActivated(GeneralSettingsView view);
        float GetGridSize(GeneralSettingsView view);

        bool IsKeepBestCreatureEnabled(GeneralSettingsView view);
        bool IsSimulateInBatchesEnabled(GeneralSettingsView view);
        int GetBatchSize(GeneralSettingsView view);

        SelectionAlgorithm GetSelectionAlgorithm(GeneralSettingsView view);
        RecombinationAlgorithm GetRecombinationAlgorithm(GeneralSettingsView view);
        MutationAlgorithm GetMutationAlgorithm(GeneralSettingsView view);
        float GetMutationRate(GeneralSettingsView view);


        void CloseButtonClicked(GeneralSettingsView view);
        void BrainButtonClicked(GeneralSettingsView view);

        void GridActivationToggled(GeneralSettingsView view, bool enabled);
        void GridSizeChanged(GeneralSettingsView view, float size);

        void KeepBestCreaturesToggled(GeneralSettingsView view, bool enabled);
        void SimulateInBatchesToggled(GeneralSettingsView view, bool enabled);
        void BatchSizeChanged(GeneralSettingsView view, int batchSize);

        void SelectionAlgorithmChanged(GeneralSettingsView view, SelectionAlgorithm algorithm);
        void RecombinationAlgorithmChanged(GeneralSettingsView view, RecombinationAlgorithm algorithm);
        void MutationAlgorithmChanged(GeneralSettingsView view, MutationAlgorithm algorithm);
        void MutationRateChanged(GeneralSettingsView view, float mutationRate);
    }

    public class GeneralSettingsView: MonoBehaviour {

        public IGeneralSettingsViewDelegate Delegate { get; set; }

        [SerializeField] private Button closeButton;
        [SerializeField] private Button brainButton;
        
        // MARK: - Editor Settings

        [SerializeField] private Toggle gridToggle;
        [SerializeField] private Slider gridSizeSlider;

        // MARK: - Simulation Settings

        [SerializeField] private Toggle keepBestToggle;
        [SerializeField] private Toggle simulateInBatchesToggle;
        [SerializeField] private InputField batchSizeInputField;
        
        [SerializeField] private Dropdown selectionAlgorithmDropdown;
        [SerializeField] private Dropdown recombinationAlgorithmDropdown;
        [SerializeField] private Dropdown mutationAlgorithmDropdown;
        [SerializeField] private Slider mutationRateSlider;

        void Start() {

            closeButton.onClick.AddListener(delegate () {
                Delegate.CloseButtonClicked(this);
            });

            brainButton.onClick.AddListener(delegate () {
                Delegate.BrainButtonClicked(this);
            });

            gridToggle.onValueChanged.AddListener(delegate (bool enabled) {
                Delegate.GridActivationToggled(this, enabled);
                Refresh();
            });

            gridSizeSlider.onValueChanged.AddListener(delegate (float gridSize) {
                Delegate.GridSizeChanged(this, gridSize);
                Refresh();
            });

            keepBestToggle.onValueChanged.AddListener(delegate (bool enabled) {
                Delegate.KeepBestCreaturesToggled(this, enabled);
                Refresh();
            });

            simulateInBatchesToggle.onValueChanged.AddListener(delegate (bool enabled) {
                Delegate.SimulateInBatchesToggled(this, enabled);
                Refresh();
            });

            batchSizeInputField.onValueChanged.AddListener(delegate (string text) {
                int batchSize = 0;
                try {
                    int.TryParse(text, out batchSize);
                } catch { 
                    Refresh();
                    return; 
                }
                Delegate.BatchSizeChanged(this, batchSize);
                Refresh();
            });

            // TODO: Hook up dropdowns
        }

        public void Refresh() {

        }
    }
}