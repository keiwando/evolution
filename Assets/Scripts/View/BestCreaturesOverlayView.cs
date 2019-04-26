using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public interface IBestCreaturesOverlayViewDelegate {

    void SelectedGeneration(BestCreaturesOverlayView view, int generation);
    void DidChangeAutoplayEnabled(BestCreaturesOverlayView view, bool enabled);
    void DidChangeAutoplayDuration(BestCreaturesOverlayView view, int duration);
    void DidClickOnPipView(BestCreaturesOverlayView view);
    
    bool IsAutoplayEnabled(BestCreaturesOverlayView view);
    int GetAutoplayDuration(BestCreaturesOverlayView view);
    
    int GetCurrentSimulationGeneration(BestCreaturesOverlayView view);
    int GetGenerationOfCurrentBest(BestCreaturesOverlayView view);
    CreatureStats GetCreatureStatsOfCurrentBest(BestCreaturesOverlayView view);
    NeuralNetworkSettings GetNetworkSettingsOfCurrentBest(BestCreaturesOverlayView view);
    int GetNumberOfNetworkInputs(BestCreaturesOverlayView view);
}

public class BestCreaturesOverlayView: MonoBehaviour {

    public IBestCreaturesOverlayViewDelegate Delegate { get; set; }

    [SerializeField]
    private InputField generationInputField;
    [SerializeField]
    private Button showPreviousGenerationButton;
    [SerializeField]
    private Button showNextGenerationButton;

    [SerializeField]
    private Text fitnessLabel;
    [SerializeField]
    private Text statsLabel;

    [SerializeField]
    private Toggle autoplayToggle;
    [SerializeField]
    private GameObject autoplaySettingsContainer;
    [SerializeField]
    private Slider autoplayDurationSlider;
    [SerializeField]
    private Text autoplayDurationLabel;

    [SerializeField]
    private Text errorLabel;

    [SerializeField]
    private RenderTexture pipRenderTexture;
    [SerializeField]
    private Text pipGenerationLabel;
    [SerializeField]
    private Button pipButton;

    // Animation
    private Coroutine errorFadeRoutine;

    void Start() {

        generationInputField.onEndEdit.AddListener(delegate (string val) {
            int generation = 0;
            if (int.TryParse(val, out generation)) {
                Delegate.SelectedGeneration(this, generation);
            } else {
                generationInputField.text = Delegate.GetGenerationOfCurrentBest(this).ToString();
            }
            Refresh();
        });

        showPreviousGenerationButton.onClick.AddListener(delegate () {
            int newGeneration = Math.Max(0, Delegate.GetGenerationOfCurrentBest(this) - 1);
            Delegate.SelectedGeneration(this, newGeneration);
            Refresh();
        });

        showNextGenerationButton.onClick.AddListener(delegate () {
            int newGeneration = Delegate.GetGenerationOfCurrentBest(this) + 1;
            Delegate.SelectedGeneration(this, newGeneration);
            Refresh();
        });

        autoplayToggle.onValueChanged.AddListener(delegate (bool enabled) {
            Delegate.DidChangeAutoplayEnabled(this, enabled);
            Refresh();
        });

        autoplayDurationSlider.onValueChanged.AddListener(delegate (float value) {
            int newDuration = Math.Max(2, (int)value);
            Delegate.DidChangeAutoplayDuration(this, newDuration);
            RefreshAutoplayDurationLabel();
        });

        pipButton.onClick.AddListener(delegate () {
            Delegate.DidClickOnPipView(this);
        });
    }

    public void Refresh() {

        int generation = Delegate.GetGenerationOfCurrentBest(this);
        generationInputField.text = generation.ToString();

        RefreshPipGenerationLabel();
        RefreshStats();

        autoplayToggle.isOn = Delegate.IsAutoplayEnabled(this);
        autoplaySettingsContainer.SetActive(autoplayToggle.isOn);
        autoplayDurationSlider.value = (float)Delegate.GetAutoplayDuration(this);
        RefreshAutoplayDurationLabel();
    }

    private void RefreshAutoplayDurationLabel() {
        autoplayDurationLabel.text = string.Format("Duration {0}s", Delegate.GetAutoplayDuration(this));
    }

    private void RefreshStats() {

        var stats = Delegate.GetCreatureStatsOfCurrentBest(this);
        var fitnessPercentage = Mathf.Round(stats.fitness * 10000f) / 100f;
        fitnessLabel.text = string.Format("Fitness: {0}%", fitnessPercentage);

        if (stats.simulationTime <= 0) {
            // This is an old simulation save where advanced stats hadn't been tracked yet.
            statsLabel.text = "";
            return;
        }

        // There are more creature stats known
        var stringBuilder = new StringBuilder();

        // Divide the lengths by 5 because of gravity scaling

        stringBuilder.AppendLine("Simulation Time:  " + stats.simulationTime + "s");
        stringBuilder.AppendLine("Average Speed:  " + (stats.averageSpeed / 5).ToString("0.00") + " m/s");
        stringBuilder.AppendLine("Horiz. distance from start:  " + (stats.horizontalDistanceTravelled / 5).ToString("0.0") + "m");
        stringBuilder.AppendLine("Vert. distance from start:  " + (stats.verticalDistanceTravelled / 5).ToString("0.0") + "m");
        stringBuilder.AppendLine("Maximum jumping height:  " + (stats.maxJumpingHeight / 5).ToString("0.0") + "m");
        stringBuilder.AppendLine("Number of bones:  " + stats.numberOfBones);
        stringBuilder.AppendLine("Number of muscles:  " + stats.numberOfMuscles);
        stringBuilder.AppendLine("Weight:  " + stats.weight + "kg");

        // Add the neural network stats:
        var networkStats = Delegate.GetNetworkSettingsOfCurrentBest(this);

        stringBuilder.AppendLine();
        stringBuilder.AppendLine("Neural Net: " + (networkStats.NumberOfIntermediateLayers + 2) + " layers");

        var numberOfInputs = Delegate.GetNumberOfNetworkInputs(this);
        int numberOfNodes = numberOfInputs + stats.numberOfMuscles;
        var layersStringBuilder = new StringBuilder();
        layersStringBuilder.Append(numberOfInputs);
        layersStringBuilder.Append(" + ");
        foreach (var layerNodeCount in networkStats.NodesPerIntermediateLayer) {
            numberOfNodes += layerNodeCount;
            layersStringBuilder.Append(layerNodeCount);
            layersStringBuilder.Append(" + ");
        }
        layersStringBuilder.Append(stats.numberOfMuscles.ToString());

        stringBuilder.Append(numberOfNodes);
        stringBuilder.Append(" nodes (");
        stringBuilder.Append(layersStringBuilder.ToString());
        stringBuilder.Append(")");

        // Display the stats
        statsLabel.text = stringBuilder.ToString();
    }

    private void RefreshPipGenerationLabel() {
        
        int generation = Delegate.GetCurrentSimulationGeneration(this);
        pipGenerationLabel.text = string.Format("Generation {0}", generation);
    }

    public RenderTexture GetPipRenderTexture() {
        return pipRenderTexture;
    }

    public void ShowErrorMessage(string message) {

        if (errorFadeRoutine != null) {
            StopCoroutine(errorFadeRoutine);
        }

        errorLabel.text = message;

        var canvasGroup = errorLabel.GetComponent<CanvasGroup>();
        errorFadeRoutine = StartCoroutine(AnimationUtils.Flash(canvasGroup, 1f, 3.5f));
    }

    /// <summary>
    /// Immediately ends all fading animations and hides the error message label.
    /// </summary>
    public void HideErrorMessage() {

        if (errorFadeRoutine != null) {
            StopCoroutine(errorFadeRoutine);
        }
        errorLabel.GetComponent<CanvasGroup>().alpha = 0f;
    }
}