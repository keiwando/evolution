using UnityEngine;
using UnityEngine.UI;
using Keiwando.UI;

namespace Keiwando.Evolution.UI {

    public interface ISimulationVisibilityOptionsViewDelegate {

        void GridVisibilityDidChange(SimulationVisibilityOptionsView view, float visibility);
        void HiddenCreatureOpacityDidChange(SimulationVisibilityOptionsView view, float opacity);
        void ShowMuscleContractionDidChange(SimulationVisibilityOptionsView view, bool showMuscleContraction);
        void ShowMusclesDidChange(SimulationVisibilityOptionsView view, bool showMuscles);

        float GetGridVisibility(SimulationVisibilityOptionsView view);
        float GetHiddenCreatureOpacity(SimulationVisibilityOptionsView view);
        bool ShouldShowMuscleContraction(SimulationVisibilityOptionsView view);
        bool ShouldShowMuscles(SimulationVisibilityOptionsView view);
    }

    [RequireComponent(typeof(SlidingContainer))]
    public class SimulationVisibilityOptionsView: MonoBehaviour {

        private struct Constants {
            public const float slidingDuration = 0.3f;
        }

        public ISimulationVisibilityOptionsViewDelegate Delegate { get; set; }

        [SerializeField] private Slider GridVisibilitySlider;
        [SerializeField] private Slider HiddenCreatureOpacitySlider;
        [SerializeField] private Toggle ShowMuscleContractionToggle;
        [SerializeField] private Toggle ShowMusclesToggle;

        [SerializeField] private Button slideInOutButton;
        [SerializeField] private Image[] ejectImages;
        [SerializeField] private CanvasGroup muscleConcentrationCanvasGroup;

        private SlidingContainer slidingContainer;
        
        void Start() {

            slidingContainer = GetComponent<SlidingContainer>();

            GridVisibilitySlider.onValueChanged.AddListener(delegate (float value) {
                Delegate.GridVisibilityDidChange(this, value);
                Refresh();
            });

            HiddenCreatureOpacitySlider.onValueChanged.AddListener(delegate (float value) {
                Delegate.HiddenCreatureOpacityDidChange(this, value);
                Refresh();
            });

            ShowMuscleContractionToggle.onValueChanged.AddListener(delegate (bool value) {
                Delegate.ShowMuscleContractionDidChange(this, value);
                Refresh();
            });

            ShowMusclesToggle.onValueChanged.AddListener(delegate (bool value) {
                Delegate.ShowMusclesDidChange(this, value);
                Refresh();
            });

            slideInOutButton.onClick.AddListener(delegate () {
                // Slide the container in and out.
                if (slidingContainer.LastSlideDirection == SlidingContainer.Direction.Up) {
                    slidingContainer.Slide(SlidingContainer.Direction.Down, Constants.slidingDuration, false);
                } else {
                    slidingContainer.Slide(SlidingContainer.Direction.Up, Constants.slidingDuration, false);
                }
                RefreshEjectImages();
            });

            // Immediately hide the menu at first
            slidingContainer.Slide(SlidingContainer.Direction.Up, 0f);
        }

        public void Refresh() {

            RefreshEjectImages();

            if (Delegate == null) {
                Debug.LogWarning("SimulationVisibilityOptionsView refreshed without a delegate!");
                return;
            }

            GridVisibilitySlider.value = Delegate.GetGridVisibility(this);
            HiddenCreatureOpacitySlider.value = Delegate.GetHiddenCreatureOpacity(this);
            ShowMusclesToggle.isOn = Delegate.ShouldShowMuscles(this);
            ShowMuscleContractionToggle.isOn = Delegate.ShouldShowMuscleContraction(this);

            // Update UI elements that depend on each other's values
            ShowMuscleContractionToggle.enabled = ShowMusclesToggle.isOn;
            muscleConcentrationCanvasGroup.alpha = ShowMusclesToggle.isOn ? 1f : 0.5f;
        }

        private void RefreshEjectImages() {

            // Flip the eject images if needed
            
            var lastDir = slidingContainer.LastSlideDirection;
            foreach (var image in ejectImages) {
                var scale = image.transform.localScale;
                if (lastDir == SlidingContainer.Direction.Up) {
                    scale.y = 1;
                } else {
                    scale.y = -1;
                }
                image.transform.localScale = scale;
            }
        }
    }
}