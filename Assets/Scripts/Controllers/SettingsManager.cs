using System;
using System.Collections.Generic;
using Keiwando.UI;
using Keiwando.Evolution.UI;

namespace Keiwando.Evolution {

    public class SettingsManager: 
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

        public NeuralNetworkSettingsUIManager neuralNetworkUIManager;

        private Evolution evolution;
        public Grid grid;

        public SettingsManager(Evolution evolution = null, 
                               NeuralNetworkSettingsUIManager neuralNetworkSettingsUIManager = null) {
            this.evolution = evolution;
            this.neuralNetworkUIManager = neuralNetworkSettingsUIManager;
        }

        public void Setup(SettingsView settingsView, bool setupForPauseScreen) {
            int tabCount = setupForPauseScreen ? 3 : 4;
            settingsView.controlGroups = new SettingControlGroup[tabCount];

            neuralNetworkUIManager.networkIsEditable = !setupForPauseScreen;

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
                            },
                            tooltip = "When enabled, joints can only be placed on a fixed grid in the creature."
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
                            },
                            tooltip = "Controls how fine the grid in the creature editor should be."
                        },
                        new SettingControl {
                            type = SettingControlType.Button,
                            name = "Reset",
                            onButtonPressed = delegate () {
                                editorSettings = EditorSettings.Default;
                            },
                            tooltip = "Resets all editor settings"
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
                                PopulationSizeDidChange(newPopulationSize);
                            }
                        },
                        tooltip = "The number of creatures to simulate in each generation."
                    },
                    new SettingControl {
                        type = SettingControlType.Input,
                        name = "Simulation Time",
                        inputValue = delegate () { return simulationSettings.SimulationTime.ToString(); },
                        onInputValueChanged = delegate (string newValue) {
                            int newSimulationTime = 0;
                            if (int.TryParse(newValue, out newSimulationTime)) {
                                SimulationTimeDidChange(newSimulationTime);
                            }
                        },
                        tooltip = "The number of seconds to simulate each creature."
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
                        },
                        disabledIf = delegate () { return setupForPauseScreen; },
                        // TODO: Explain each task here.
                        tooltip = "The task that the creatures are trying to learn." + (setupForPauseScreen ? "\nThis cannot be changed during a simulation." : "")
                    },
                    new SettingControl {
                        type = SettingControlType.Toggle,
                        name = "Keep Best Creatures",
                        toggleValue = delegate () { return simulationSettings.KeepBestCreatures; },
                        onToggleValueChanged = delegate (bool isOn) {
                            var settings = simulationSettings;
                            settings.KeepBestCreatures = isOn;
                            simulationSettings = settings;
                        },
                        tooltip = "When this option is enabled, the two best creatures of a simulation run are copied to the next generation without any modifications (no recombination or mutation)."
                    },
                    new SettingControl {
                        type = SettingControlType.Toggle,
                        name = "Simulate In Batches",
                        toggleValue = delegate () { return simulationSettings.SimulateInBatches; },
                        onToggleValueChanged = delegate (bool isOn) {
                            var settings = simulationSettings;
                            settings.SimulateInBatches = isOn;
                            simulationSettings = settings;
                        },
                        tooltip = "Instead of simulating all of the creatures of one generation at once, you have the option to simulate them in smaller batches. This will be easier on the CPU but it will also take a longer amount of time to finish simulating each generation."
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
                        },
                        disabledIf = delegate () { return !simulationSettings.SimulateInBatches; },
                        tooltip = "The number of creatures of the same generation to simulate at once."
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
                        },
                        tooltip = @"
<size=20><b>Selection Algorithm</b></size>


Defines how creatures are selected for survival and reproduction.


<b>Uniform</b>

Every creature has the same chances of being selected.


<b>Fitness Proportional</b>

The selection probability is proportional to the fitness of the creature.


<b>Rank
Proportional</b>

The selection probability is proportional to the rank of the fitness of the creature.


<b>Tournament</b>

Chooses
a group of creatures uniformly at random and then selects the best one of that group.
"
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
                        },
                        tooltip = @"
<size=20><b>Recombination Algorithm</b></size>


Defines how two parent chromosomes are recombined into one offspring.


<b>One Point</b>

Cuts the parent chromosomes at the same random index and uses one part from each parent.


<b>Multi Point</b>

Cuts the parent chromosomes at multiple random indices and alternatingly chooses parts from the parent chromosomes.


<b>Uniform</b>

Chooses each bit at random from either parent chromosome.
"
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
                        },
                        tooltip = @"
<size=20><b>Mutation Algorithm</b></size>

Defines how random changes are applied to offspring chromosomes.


<b>Chunk</b>

Changes a random number of consecutive values in the chromosome.


<b>Global</b>

Changes the value at each index with a random probability.


<b>Inversion</b>

Chooses a random start and end index and inverts the order of values inbetween.'                        
"
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
                        },
                        tooltip = "The probability for each offspring to receive any amount of random mutation."
                    },
                    new SettingControl {
                        type = SettingControlType.Button,
                        name = "Reset",
                        onButtonPressed = delegate () {
                            simulationSettings = SimulationSettings.Default;
                        },
                        // Disabled so you don't change the objective with it.
                        disabledIf = delegate () { return setupForPauseScreen; },
                        tooltip = "Resets all simulation settings to their default values."
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

                                    neuralNetworkUIManager.Refresh();
                                }
                            }
                        },
                        disabledIf = delegate () { return setupForPauseScreen; },
                        tooltip = "The total number of layers in the neural network that controls the creature's muscles." + (setupForPauseScreen ? "\nThis cannot be changed during a simulation." : "")
                    },
                    new SettingControl {
                        type = SettingControlType.Button,
                        name = "Reset",
                        onButtonPressed = delegate () {
                            networkSettings = NeuralNetworkSettings.Default;
                            
                            neuralNetworkUIManager.Refresh();
                        },
                        disabledIf = delegate () { return setupForPauseScreen; },
                        tooltip = "Resets the neural network to its default state."
                    }
                },
                anciliaryView = neuralNetworkUIManager.neuralNetworkUIRootContainer
            };

            // TODO: Add credits tab
            settingsView.controlGroups[nextTabIndex++] = new SettingControlGroup {
                name = "Credits",
                controls = new SettingControl[] {
                    new SettingControl {
                        type = SettingControlType.Button,
                        name = "Credits"
                        // TODO: Add tooltip
                    },
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
            int newPopulationSize = Math.Clamp(value, 2, 1000);
            var settings = simulationSettings;
            settings.PopulationSize = newPopulationSize;
            settings.BatchSize = Math.Clamp(settings.BatchSize, 1, settings.PopulationSize);
            simulationSettings = settings;
        }

        public void SimulationTimeDidChange(int simulationTime) {
            int newSimulationTime = Math.Clamp(simulationTime, 1, 100000);
            var settings = simulationSettings;
            settings.SimulationTime = newSimulationTime;
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

