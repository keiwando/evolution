using UnityEngine;
using UnityEngine.UI;

public interface IEvolutionOverlayViewDelegate {

    void FocusOnNextCreature();
    void FocusOnPreviousCreature();
    void DidClickOnPipView(EvolutionOverlayView view);

    int GetCurrentGenerationNumber(EvolutionOverlayView view);
    int GetGurrentBestOfGenerationNumber(EvolutionOverlayView view);
    int GetCurrentBatchNumber(EvolutionOverlayView view);
    int GetTotalBatchCount(EvolutionOverlayView view);
    bool IsSimulatingInBatches(EvolutionOverlayView view);
}

public class EvolutionOverlayView: MonoBehaviour {

    public IEvolutionOverlayViewDelegate Delegate { get; set; }

    [SerializeField]
    private Text generationLabel;

    [SerializeField]
    private Button focusOnPreviousButton;
    [SerializeField]
    private Button focusOnNextButton;

    [SerializeField]
    private GameObject pipContainer;
    [SerializeField]
    private RenderTexture pipRenderTexture;
    [SerializeField]
    private Text pipBestOfGenLabel;
    [SerializeField]
    private Button pipButton;

    void Start() {
        
        focusOnNextButton.onClick.AddListener(delegate () {
            Delegate.FocusOnNextCreature();
        });
        focusOnPreviousButton.onClick.AddListener(delegate () {
            Delegate.FocusOnPreviousCreature();
        });
        pipButton.onClick.AddListener(delegate () {
            Delegate.DidClickOnPipView(this);
        });
    }

    public void Refresh() {

        RefreshGenerationLabel();
        RefreshPipGenerationLabel();
        pipContainer.SetActive(Delegate.GetCurrentGenerationNumber(this) > 1);
    }

    public RenderTexture GetPipRenderTexture() {
        return pipRenderTexture;
    }

    private void RefreshGenerationLabel() {

        int generation = Delegate.GetCurrentGenerationNumber(this);
        int currentBatch = Delegate.GetCurrentBatchNumber(this);
        int totalBatches = Delegate.GetTotalBatchCount(this);
        bool batchesEnabled = Delegate.IsSimulatingInBatches(this);

        var text = string.Format("Generation {0}", generation);
        if (batchesEnabled) {
            text += string.Format(" ({0}/{1})", currentBatch, totalBatches);
        }

        generationLabel.text = text;
    }

    private void RefreshPipGenerationLabel() {
        // The current generation of the creature in the best creatures view
        int generation = Delegate.GetGurrentBestOfGenerationNumber(this);
        pipBestOfGenLabel.text = string.Format("Best of Gen. {0}", generation);
    }
}