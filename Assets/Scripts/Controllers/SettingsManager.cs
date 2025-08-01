using System;
using System.Collections.Generic;
using Keiwando.UI;
using Keiwando.Evolution.UI;

namespace Keiwando.Evolution {

    public class SettingsManager: 
        ISettingsViewControllerDelegate,
        IBasicSettingsViewDelegate {

        private EditorSettings editorSettings {
            get => EditorStateManager.EditorSettings;
            set => EditorStateManager.EditorSettings = value;
        }
        private SimulationSettings simulationSettings {
            get {
                if (evolution != null) {
                    return evolution.SettingsForNextGeneration;
                } else {
                    return EditorStateManager.SimulationSettings;
                }
            }
            set {
                EditorStateManager.SimulationSettings = value;
                if (evolution != null) 
                    evolution.SettingsForNextGeneration = value;
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

        public void Setup(SettingsView settingsView, bool setupForPauseScreen) {
            int tabCount = setupForPauseScreen ? 3 : 4;
            settingsView.controlGroups = new SettingControlGroup[tabCount];

            int nextTabIndex = 0;
            if (!setupForPauseScreen) {
                settingsView.controlGroups[nextTabIndex] = new SettingControlGroup {
                    name = "Editor",
                    controls = new SettingControl[] {
                        new SettingControl {
                            type = SettingControlType.Toggle,
                            name = "Grid",
                            toggleValue = delegate () { return editorSettings.GridEnabled; },
                            onToggleValueChanged = delegate (bool isOn) { 
                                var settings = editorSettings;
                                settings.GridEnabled = isOn;
                                editorSettings = settings;
                            }
                        },
                        new SettingControl {
                            type = SettingControlType.Slider,
                            name = "Grid Size",
                            sliderMinValue = EditorSettings.MIN_GRID_SIZE,
                            sliderMaxValue = EditorSettings.MAX_GRID_SIZE,
                            sliderValue = delegate () { return editorSettings.GridSize; },
                            sliderFormattedValue = delegate () { 
                                return editorSettings.GridSize.ToString("0.0");
                            },
                            onSliderValueChanged = delegate (float value) {
                                var settings = editorSettings;
                                settings = editorSettings;
                                settings.GridSize = Math.Clamp(value, min: EditorSettings.MIN_GRID_SIZE, max: EditorSettings.MAX_GRID_SIZE);
                                editorSettings = settings;
                            }
                        },
                        new SettingControl {
                            type = SettingControlType.Button,
                            name = "Reset",
                            onButtonPressed = delegate () {
                                editorSettings = EditorSettings.Default;
                            }
                        }
                    }
                };
                nextTabIndex++;
            }

            settingsView.controlGroups[nextTabIndex++] = new SettingControlGroup {
                name = "Simulation",
                controls = new SettingControl [] {
                    new SettingControl {
                        type = SettingControlType.Input,
                        name = "Population Size",
                        inputValue = delegate () { return simulationSettings.PopulationSize.ToString(); },
                        onInputValueChanged = delegate (string newValue) {
                            int newPopulationSize = 0;
                            if (int.TryParse(newValue, out newPopulationSize)) {
                                newPopulationSize = Math.Clamp(newPopulationSize, 2, 1000);
                                var settings = simulationSettings;
                                settings.PopulationSize = newPopulationSize;
                                settings.BatchSize = Math.Clamp(settings.BatchSize, 1, settings.PopulationSize);
                                simulationSettings = settings;
                            }
                        }
                    },
                    new SettingControl {
                        type = SettingControlType.Input,
                        name = "SimulationTime",
                        inputValue = delegate () { return simulationSettings.SimulationTime.ToString(); },
                        onInputValueChanged = delegate (string newValue) {
                            int newSimulationTime = 0;
                            if (int.TryParse(newValue, out newSimulationTime)) {
                                newSimulationTime = Math.Clamp(newSimulationTime, 1, 100000);
                                var settings = simulationSettings;
                                settings.SimulationTime = newSimulationTime;
                                simulationSettings = settings;
                            }
                        }
                    },
                    new SettingControl {
                        type = SettingControlType.Multiselect,
                        name = "Task",
                        multiselectNames = ObjectiveUtil.GetAllObjectiveNames(),
                        multiselectSelectedIndex = delegate () {
                            Objective task = simulationSettings.Objective;
                            int taskIndex = Array.IndexOf(ObjectiveUtil.ALL_OBJECTIVES, task);
                            return taskIndex;
                        },
                        onMultiselectIndexChanged = delegate (int index) {
                            var settings = simulationSettings;
                            settings.Objective = ObjectiveUtil.ALL_OBJECTIVES[index];
                            simulationSettings = settings;
                        }
                    },
                    new SettingControl {
                        type = SettingControlType.Toggle,
                        name = "Keep Best Creatures",
                        toggleValue = delegate () { return simulationSettings.KeepBestCreatures; },
                        onToggleValueChanged = delegate (bool isOn) {
                            var settings = simulationSettings;
                            settings.KeepBestCreatures = isOn;
                            simulationSettings = settings;
                        } 
                    },
                    new SettingControl {
                        type = SettingControlType.Toggle,
                        name = "Simulate In Batches",
                        toggleValue = delegate () { return simulationSettings.SimulateInBatches; },
                        onToggleValueChanged = delegate (bool isOn) {
                            var settings = simulationSettings;
                            settings.SimulateInBatches = isOn;
                            simulationSettings = settings;
                        }
                    },
                    new SettingControl {
                        type = SettingControlType.Input,
                        name = "Batch Size",
                        inputValue = delegate () { return simulationSettings.BatchSize.ToString(); },
                        onInputValueChanged = delegate (string newValue) {
                            int newBatchSize = 0;
                            if (int.TryParse(newValue, out newBatchSize)) {
                                newBatchSize = Math.Clamp(newBatchSize, 1, simulationSettings.BatchSize);
                                var settings = simulationSettings;
                                settings.BatchSize = newBatchSize;
                                simulationSettings = settings;
                            }
                        }
                    },
                    new SettingControl {
                        type = SettingControlType.Multiselect,
                        name = "Selection Algorithm",
                        multiselectNames = ALL_SELECTION_ALGORITHM_NAMES,
                        multiselectSelectedIndex = delegate () {
                            return Array.IndexOf(ALL_SELECTION_ALGORITHMS, simulationSettings.SelectionAlgorithm);
                        },
                        onMultiselectIndexChanged = delegate (int index) {
                            var settings = simulationSettings;
                            settings.SelectionAlgorithm = ALL_SELECTION_ALGORITHMS[index];
                            simulationSettings = settings;
                        }
                    },
                    new SettingControl {
                        type = SettingControlType.Multiselect,
                        name = "Recombination Algorithm",
                        multiselectNames = ALL_RECOMBINATION_ALGORITHM_NAMES,
                        multiselectSelectedIndex = delegate () {
                            return Array.IndexOf(ALL_RECOMBINATION_ALGORITHMS, simulationSettings.RecombinationAlgorithm);
                        },
                        onMultiselectIndexChanged = delegate (int index) {
                            var settings = simulationSettings;
                            settings.RecombinationAlgorithm = ALL_RECOMBINATION_ALGORITHMS[index];
                            simulationSettings = settings;
                        }
                    },
                    new SettingControl {
                        type = SettingControlType.Multiselect,
                        name = "Mutation Algorithm",
                        multiselectNames = ALL_MUTATION_ALGORITHM_NAMES,
                        multiselectSelectedIndex = delegate () {
                            return Array.IndexOf(ALL_MUTATION_ALGORITHMS, simulationSettings.MutationAlgorithm);
                        },
                        onMultiselectIndexChanged = delegate (int index) {
                            var settings = simulationSettings;
                            settings.MutationAlgorithm = ALL_MUTATION_ALGORITHMS[index];
                            simulationSettings = settings;
                        }
                    },
                    new SettingControl {
                        type = SettingControlType.Slider,
                        name = "Mutation Rate",
                        sliderMinValue = 0,
                        sliderMaxValue = 1,
                        sliderValue = delegate () { return simulationSettings.MutationRate; },
                        sliderFormattedValue = delegate () { return $"{((int)(simulationSettings.MutationRate * 100))}%"; },
                        onSliderValueChanged = delegate (float value) {
                            var settings = simulationSettings;
                            settings.MutationRate = Math.Clamp(value, 0f, 1f);
                            simulationSettings = settings;
                        }
                    },
                    new SettingControl {
                        type = SettingControlType.Button,
                        name = "Reset",
                        onButtonPressed = delegate () {
                            simulationSettings = SimulationSettings.Default;
                        }
                    }
                }
            };
            
            settingsView.controlGroups[nextTabIndex++] = new SettingControlGroup {
                name = "Neural Network",
                controls = new SettingControl[] {
                    new SettingControl {
                        type = SettingControlType.Input,
                        name = "Number of Layers",
                        inputValue = delegate () { return (networkSettings.NumberOfIntermediateLayers + 2).ToString(); },
                        onInputValueChanged = delegate (string newTextValue) {
                            int numberOfLayers = 2;
                            if (int.TryParse(newTextValue, out numberOfLayers)) {
                                numberOfLayers = Math.Clamp(numberOfLayers, 3, NeuralNetworkSettings.MAX_LAYERS);
                                var settings = networkSettings;

                                var oldNumber = settings.NumberOfIntermediateLayers + 2;

                                if (numberOfLayers != oldNumber) {
                                    // Number was changed
                                    var layerSizes = new List<int>(settings.NodesPerIntermediateLayer);

                                    if (numberOfLayers > oldNumber) {
                                        // Duplicate the last layer
                                        for ( int i = 0; i < numberOfLayers - oldNumber; i++)
                                            layerSizes.Add(layerSizes[layerSizes.Count - 1]);
                                    } else {
                                        for (int i = 0; i < oldNumber - numberOfLayers; i++)
                                            layerSizes.RemoveAt(layerSizes.Count - 1);	
                                    }

                                    settings.NodesPerIntermediateLayer = layerSizes.ToArray();
                                    networkSettings = settings;
                                }
                            }
                        }
                    },
                    new SettingControl {
                        type = SettingControlType.Button,
                        name = "Reset",
                        onButtonPressed = delegate () {
                            networkSettings = NeuralNetworkSettings.Default;
                        }
                    }
                }
            };

            // TODO: Add credits tab
            settingsView.controlGroups[nextTabIndex++] = new SettingControlGroup {
                name = "Credits",
                controls = new SettingControl[] {
                    new SettingControl {
                        type = SettingControlType.Button,
                        name = "Impressum",
                        onButtonPressed = delegate () {
                            // TODO: Implement
                        }
                    }
                }
            };

            settingsView.SetupControls();
        }

        // TODO: Remove all of this once unused

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

        public void SimulationTimeDidChange(int time) {
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
            settings.BatchSize = Math.Min(Math.Max(batchSize, 1), settings.PopulationSize);
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

        // MARK: - IBasicSettingsViewDelegate

        public int GetPopulationSize() {
            return simulationSettings.PopulationSize;
        }

        public int GetGenerationDuration() {
            return simulationSettings.SimulationTime;
        }

        public Objective GetObjective() {
            return simulationSettings.Objective;
        }

        public void PopulationSizeDidChange(int value) {
            var settings = simulationSettings;
            settings.PopulationSize = Math.Max(2, value);
            simulationSettings = settings;
        }

        public void ObjectiveDidChange(Objective objective) {
            var settings = simulationSettings;
            settings.Objective = objective;
            simulationSettings = settings;
        }

        private static SelectionAlgorithm[] ALL_SELECTION_ALGORITHMS = new SelectionAlgorithm[] {
            SelectionAlgorithm.FitnessProportional,
            SelectionAlgorithm.RankProportional,
            SelectionAlgorithm.TournamentSelection,
            SelectionAlgorithm.Uniform
        };
        private static string[] ALL_SELECTION_ALGORITHM_NAMES = new string[] {
            "Fitness Proportional",
            "Rank Proportional",
            "Tournament",
            "Uniform"
        };
        private static RecombinationAlgorithm[] ALL_RECOMBINATION_ALGORITHMS = new RecombinationAlgorithm[] {
            RecombinationAlgorithm.OnePointCrossover,
            RecombinationAlgorithm.MultiPointCrossover,
            RecombinationAlgorithm.UniformCrossover
        };
        private static string[] ALL_RECOMBINATION_ALGORITHM_NAMES = new string[] {
            "One Point",
            "Multi Point",
            "Uniform"
        };
        private static MutationAlgorithm[] ALL_MUTATION_ALGORITHMS = new MutationAlgorithm[] {
            MutationAlgorithm.Chunk,
            MutationAlgorithm.Global,
            MutationAlgorithm.Inversion
        };
        private static string[] ALL_MUTATION_ALGORITHM_NAMES = new string[] {
            "Chunk",
            "Global",
            "Inversion"
        };
    }
}

