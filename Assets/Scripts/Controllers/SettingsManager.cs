using System;
using System.Collections.Generic;
using UnityEngine;
using Keiwando.UI;
using Keiwando.Evolution.UI;

namespace Keiwando.Evolution {

    public enum WindowMode: int {
        Windowed = 0,
        Fullscreen = 1
    }

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
                                refreshGrid();
                            },
                            tooltip = SettingsTooltips.GRID
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
                                refreshGrid();
                            },
                            tooltip = SettingsTooltips.GRID_SIZE
                        },
                        new SettingControl {
                            type = SettingControlType.Button,
                            name = "Reset",
                            onButtonPressed = delegate () {
                                editorSettings = EditorSettings.Default;
                                refreshGrid();
                            },
                            tooltip = SettingsTooltips.EDITOR_RESET
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
                        tooltip = SettingsTooltips.POPULATION_SIZE
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
                        tooltip = SettingsTooltips.SIMULATION_TIME
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
                        tooltip = SettingsTooltips.TASK + (setupForPauseScreen ? "\nThis cannot be changed during a simulation." : "")
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
                        tooltip = SettingsTooltips.KEEP_BEST_CREATURES
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
                        tooltip = SettingsTooltips.SIMULATE_IN_BATCHES
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
                        tooltip = SettingsTooltips.BATCH_SIZE
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
                        tooltip = SettingsTooltips.SELECTION_ALGORITHM
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
                        tooltip = SettingsTooltips.RECOMBINATION_ALGORITHM
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
                        tooltip = SettingsTooltips.MUTATION_ALGORITHM
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
                        tooltip = SettingsTooltips.MUTATION_RATE
                    },
                    new SettingControl {
                        type = SettingControlType.Button,
                        name = "Reset",
                        onButtonPressed = delegate () {
                            simulationSettings = SimulationSettings.Default;
                        },
                        // Disabled so you don't change the objective with it.
                        disabledIf = delegate () { return setupForPauseScreen; },
                        tooltip = SettingsTooltips.SIMULATION_RESET
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
                        tooltip = SettingsTooltips.NETWORK_NUMBER_OF_LAYERS + (setupForPauseScreen ? "\nThis cannot be changed during a simulation." : "")
                    },
                    new SettingControl {
                        type = SettingControlType.Button,
                        name = "Reset",
                        onButtonPressed = delegate () {
                            networkSettings = NeuralNetworkSettings.Default;
                            
                            neuralNetworkUIManager.Refresh();
                        },
                        disabledIf = delegate () { return setupForPauseScreen; },
                        tooltip = SettingsTooltips.NETWORK_RESET
                    }
                },
                anciliaryView = neuralNetworkUIManager.neuralNetworkUIRootContainer
            };

            bool showQuitButton = false;
            #if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_EDITOR
            showQuitButton = true;
            #endif
            bool showWindowModeSelector = true;
            #if UNITY_IOS
            showWindowModeSelector = false;
            #endif
            int generalSettingsCount = 4;
            if (showWindowModeSelector) {
                generalSettingsCount += 1;
            }
            if (showQuitButton) {
                generalSettingsCount += 1;
            }
            SettingControl[] generalSettingsControls = new SettingControl[generalSettingsCount];
            int generalSettingIndex = 0;
            if (showWindowModeSelector) {
                generalSettingsControls[generalSettingIndex++] = new SettingControl {
                    type = SettingControlType.Multiselect,
                    name = "Window Mode",
                    multiselectNames = ALL_WINDOW_MODE_NAMES,
                    multiselectSelectedIndex = delegate () {
                        return Array.IndexOf(ALL_WINDOW_MODES, GetCurrentWindowMode());
                    },
                    onMultiselectIndexChanged = delegate (int index) {
                        WindowMode windowMode = ALL_WINDOW_MODES[index];
                        SettingsManager.SetWindowMode(windowMode);
                    } 
                };
            }
            generalSettingsControls[generalSettingIndex++] = new SettingControl {
                type = SettingControlType.Label,
                name = "Credits",
                tooltip = SettingsTooltips.CREDITS
            };
            generalSettingsControls[generalSettingIndex++] = new SettingControl {
                type = SettingControlType.Label,
                name = "Version",
                tooltip = string.Format("<size=24>{0}</size>", Application.version.ToString())
            };
            generalSettingsControls[generalSettingIndex++] = new SettingControl {
                type = SettingControlType.Button,
                name = "Source Code",
                onButtonPressed = delegate () {
                    Application.OpenURL("https://github.com/keiwando/evolution");
                },
                tooltip = SettingsTooltips.SOURCE_CODE
            };
            generalSettingsControls[generalSettingIndex++] = new SettingControl {
                type = SettingControlType.Label,
                name = "Impressum",
                tooltip = SettingsTooltips.IMPRESSUM()
            };
            if (showQuitButton) {
                generalSettingsControls[generalSettingIndex++] = new SettingControl {
                    type = SettingControlType.Button,
                    name = "Quit Game",
                    onButtonPressed = delegate () {
                        Application.Quit();
                    },
                    tooltip = SettingsTooltips.QUIT_GAME
                };
            }
            settingsView.controlGroups[nextTabIndex++] = new SettingControlGroup {
                name = "General",
                controls = generalSettingsControls
            };

            settingsView.SetupControls();
        }

        private void refreshGrid() {
            grid.gameObject.SetActive(editorSettings.GridEnabled);
            grid.Size = editorSettings.GridSize;
        }

        public static void SetWindowMode(WindowMode windowMode) {
            #if !UNITY_IOS
            switch (windowMode) {
                case (WindowMode.Windowed): {
                    Screen.fullScreenMode = FullScreenMode.Windowed;
                    int width = Screen.currentResolution.width / 2;
                    int height = Screen.currentResolution.height / 2;
                    if (width > 0 && height > 0) {
                        Screen.SetResolution(width, height, fullscreen: false);
                    }
                    Screen.fullScreen = false;
                    break;
                }
                case (WindowMode.Fullscreen): {
                    Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                    int width = Screen.currentResolution.width;
                    int height = Screen.currentResolution.height;
                    if (width > 0 && height > 0) {
                        Screen.SetResolution(width, height, fullscreen: true);
                    }
                    Screen.fullScreen = true;
                    break;
                }
                default:
                    break;
            }
            #endif
        }

        public static WindowMode GetCurrentWindowMode() {
            if (Screen.fullScreen) {
                return WindowMode.Fullscreen;
            } else {
                return WindowMode.Windowed;
            }
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

        private static string[] ALL_WINDOW_MODE_NAMES = new string[] {
            "Windowed",
            "Fullscreen"
        };
        private static WindowMode[] ALL_WINDOW_MODES = new WindowMode[] {
            WindowMode.Windowed,
            WindowMode.Fullscreen
        };
    }
}

internal class SettingsTooltips {

    public static string GRID = "When enabled, joints can only be placed on a fixed grid in the creature.";
    public static string GRID_SIZE = "Controls how fine the grid in the creature editor should be.";
    public static string EDITOR_RESET = "Resets all editor settings.";

    public static string POPULATION_SIZE = "The number of creatures to simulate in each generation.";
    public static string SIMULATION_TIME = "The number of seconds to simulate each creature.";
    public static string TASK = @"<size=20>Task</size>

The task that the creatures are trying to learn. This objective determines how the fitness score of each creature is calculated.


<size=18>Running</size>

Move as far to the right as possible.

<size=18>Jumping</size>

Jump as high as possible.

<size=18>Obstacle Jump</size>

Jump to avoid a rolling ball.

<size=18>Climbing</size>

Climb up a staircase.

<size=18>Flying</size>

Fly as high as possible and avoid touching the ground for as long as possible.
"; 
    public static string KEEP_BEST_CREATURES = "When this option is enabled, the two best creatures of a simulation run are copied to the next generation without any modifications (no recombination or mutation).";
    public static string SIMULATE_IN_BATCHES =  "Instead of simulating all of the creatures of one generation at once, you have the option to simulate them in smaller batches. This will be easier on the CPU but it will also take a longer amount of time to finish simulating each generation.";
    public static string BATCH_SIZE = "The number of creatures of the same generation to simulate at once.\nEnable \"Simulate in Batches\" to edit this value.";
    public static string SELECTION_ALGORITHM = @"<size=20><b>Selection Algorithm</b></size>


Defines how creatures are selected for survival and reproduction.


<size=18>Uniform</size>

Every creature has the same chances of being selected.


<size=18>Fitness Proportional</size>

The selection probability is proportional to the fitness of the creature.


<size=18>Rank Proportional</size>

The selection probability is proportional to the rank of the fitness of the creature.


<size=18>Tournament</size>

Chooses a group of creatures uniformly at random and then selects the best one of that group.
";
    public static string RECOMBINATION_ALGORITHM = @"<size=20><b>Recombination Algorithm</b></size>


Defines how two parent chromosomes are recombined into one offspring.


<size=18>One Point</size>

Cuts the parent chromosomes at the same random index and uses one part from each parent.


<size=18>Multi Point</size>

Cuts the parent chromosomes at multiple random indices and alternatingly chooses parts from the parent chromosomes.


<size=18>Uniform</size>

Chooses each bit at random from either parent chromosome.
";
    public static string MUTATION_ALGORITHM = @"<size=20><b>Mutation Algorithm</b></size>

Defines how random changes are applied to offspring chromosomes.


<size=18>Chunk</size>

Changes a random number of consecutive values in the chromosome.


<size=18>Global</size>

Changes the value at each index with a random probability.


<size=18>Inversion</size>

Chooses a random start and end index and inverts the order of values inbetween.       
";
    public static string MUTATION_RATE = "The probability for each offspring to receive any amount of random mutation.";
    public static string SIMULATION_RESET = "Resets all simulation settings to their default values.";

    public static string NETWORK_NUMBER_OF_LAYERS = "The total number of layers in the neural network that controls the creature's muscles.";
    public static string NETWORK_RESET = "Resets the neural network to its default state.";

    public static string CREDITS = @"Made by
<size=22>Keiwan Donyagard</size>

<size=16>Website:</size>
keiwando.com

<size=16>Evolution Website:</size>
keiwando.com/evolution

<size=16>Contact:</size>
keiwando.com/contact
";
    public static string SOURCE_CODE = "github.com/keiwando/evolution";

    private static string IMPRESSUM_B64 = "QU5HQUJFTiBHRU3DhFNTIMKnIDUgVE1HOgpLZWl3YW4gRG9ueWFnYXJkIFZhamVkCgpLb250YWt0CmtlaXdhbi5kb255YWdhcmRAZ21haWwuY29tCkZheDogKzQ5IDIzMSA5ODE5NDgzMQpTY2jDvHR6ZW5zdHJhw59lIDk3CjQ0MTQ3IERvcnRtdW5k";
    private static string IMPRESSUM_TXT = null;
    public static string IMPRESSUM() {
        if (IMPRESSUM_TXT == null) {
            byte[] decodedBytes = System.Convert.FromBase64String(IMPRESSUM_B64);
            IMPRESSUM_TXT = System.Text.Encoding.UTF8.GetString (decodedBytes);
        }
        return IMPRESSUM_TXT;
    }
    
    public static string QUIT_GAME = "Quits the game. Unsaved changes are lost.";
}

