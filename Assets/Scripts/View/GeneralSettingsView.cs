using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Keiwando.Evolution.UI {

    public interface IGeneralSettingsViewDelegate {

        bool IsGridActivated(GeneralSettingsView view);
        float GetGridSize(GeneralSettingsView view);
        float GetMinGridSize(GeneralSettingsView view);
        float GetMaxGridSize(GeneralSettingsView view);

        bool IsKeepBestCreatureEnabled(GeneralSettingsView view);
        bool IsSimulateInBatchesEnabled(GeneralSettingsView view);
        int GetBatchSize(GeneralSettingsView view);

        SelectionAlgorithm GetSelectionAlgorithm(GeneralSettingsView view);
        RecombinationAlgorithm GetRecombinationAlgorithm(GeneralSettingsView view);
        MutationAlgorithm GetMutationAlgorithm(GeneralSettingsView view);
        float GetMutationRate(GeneralSettingsView view);

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

        private struct DropdownData<T> {
            public int Index { get; set; }
            public T Value { get; set; }
            public string Label { get; set; }
        }

        private readonly DropdownData<SelectionAlgorithm>[] selectionAlgorithms = new [] {
            new DropdownData<SelectionAlgorithm>() { 
                Index = 0,
                Value = SelectionAlgorithm.FitnessProportional,
                Label = "Fitness Proportional"
            },
            new DropdownData<SelectionAlgorithm>() { 
                Index = 1,
                Value = SelectionAlgorithm.RankProportional,
                Label = "Rank Proportional"
            },
            new DropdownData<SelectionAlgorithm>() { 
                Index = 2,
                Value = SelectionAlgorithm.TournamentSelection,
                Label = "Tournament"
            },
            new DropdownData<SelectionAlgorithm>() { 
                Index = 3,
                Value = SelectionAlgorithm.Uniform,
                Label = "Uniform"
            },
        };

        private readonly DropdownData<RecombinationAlgorithm>[] recombinationAlgorithms = new [] {
            new DropdownData<RecombinationAlgorithm>() {
                Index = 0,
                Value = RecombinationAlgorithm.OnePointCrossover,
                Label = "One Point"
            },
            new DropdownData<RecombinationAlgorithm>() {
                Index = 1,
                Value = RecombinationAlgorithm.MultiPointCrossover,
                Label = "Multi Point"
            },
            new DropdownData<RecombinationAlgorithm>() {
                Index = 2,
                Value = RecombinationAlgorithm.UniformCrossover,
                Label = "Uniform"
            }
        };

        private readonly DropdownData<MutationAlgorithm>[] mutationAlgorithms = new [] {
            new DropdownData<MutationAlgorithm>() {
                Index = 0,
                Value = MutationAlgorithm.ChunkFlip,
                Label = "Chunk Flip"
            },
            new DropdownData<MutationAlgorithm>() {
                Index = 1,
                Value = MutationAlgorithm.Global,
                Label = "Global"
            },
            new DropdownData<MutationAlgorithm>() {
                Index = 2,
                Value = MutationAlgorithm.Inversion,
                Label = "Inversion"
            },
        };

        void Start() {

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

            selectionAlgorithmDropdown.options = selectionAlgorithms.Select(
                x => new Dropdown.OptionData(x.Label)
            ).ToList();
            
            selectionAlgorithmDropdown.onValueChanged.AddListener(delegate (int index) {
                var algorithm = selectionAlgorithms[index].Value;
                Delegate.SelectionAlgorithmChanged(this, algorithm);
                Refresh();
            });

            recombinationAlgorithmDropdown.options = recombinationAlgorithms.Select(
                x => new Dropdown.OptionData(x.Label)
            ).ToList();

            recombinationAlgorithmDropdown.onValueChanged.AddListener(delegate (int index) {
                var algorithm = recombinationAlgorithms[index].Value;
                Delegate.RecombinationAlgorithmChanged(this, algorithm);
                Refresh();
            });

            mutationAlgorithmDropdown.options = mutationAlgorithms.Select(
                x => new Dropdown.OptionData(x.Label)
            ).ToList();

            mutationAlgorithmDropdown.onValueChanged.AddListener(delegate (int index) {
                var algorithm = mutationAlgorithms[index].Value;
                Delegate.MutationAlgorithmChanged(this, algorithm);
                Refresh();
            });

            mutationRateSlider.onValueChanged.AddListener(delegate (float value) {
                Delegate.MutationRateChanged(this, value);
                Refresh();
            });
        }

        public void Refresh() {

            if (Delegate == null) return;

            gridToggle.isOn = Delegate.IsGridActivated(this);
            gridSizeSlider.value = Delegate.GetGridSize(this);
            gridSizeSlider.minValue = Delegate.GetMinGridSize(this);
            gridSizeSlider.maxValue = Delegate.GetMaxGridSize(this);

            keepBestToggle.isOn = Delegate.IsKeepBestCreatureEnabled(this);
            simulateInBatchesToggle.isOn = Delegate.IsSimulateInBatchesEnabled(this);
            batchSizeInputField.text = Delegate.GetBatchSize(this).ToString();

            var selectionAlgorithm = Delegate.GetSelectionAlgorithm(this);
            selectionAlgorithmDropdown.value = selectionAlgorithms.Where(
                x => x.Value == selectionAlgorithm
            ).FirstOrDefault().Index;

            var recombinationAlgorithm = Delegate.GetRecombinationAlgorithm(this);
            recombinationAlgorithmDropdown.value = recombinationAlgorithms.Where(
                x => x.Value == recombinationAlgorithm
            ).FirstOrDefault().Index;

            var mutationAlgorithm = Delegate.GetMutationAlgorithm(this);
            mutationAlgorithmDropdown.value = mutationAlgorithms.Where(
                x => x.Value == mutationAlgorithm
            ).FirstOrDefault().Index;

            mutationRateSlider.value = Delegate.GetMutationRate(this);
        }
    }
}