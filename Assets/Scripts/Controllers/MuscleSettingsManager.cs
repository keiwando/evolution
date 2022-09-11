using Keiwando.Evolution.UI;
using Keiwando.UI;

namespace Keiwando.Evolution {

    public class MuscleSettingsManager: BodyComponentSettingsManager {

        private const float MIN_STRENGTH = 0f;
        private const float MAX_STRENGTH = 4500f;

        private Muscle muscle;
        private AdvancedBodyControlsViewController viewController;
        private LabelledSlider strengthSlider;
        private LabelledToggle canExpandToggle;

        public MuscleSettingsManager(Muscle muscle, AdvancedBodyControlsViewController viewController): base() {
            this.muscle = muscle;
            this.viewController = viewController;

            viewController.Reset();
            viewController.SetTitle("Muscle Settings");
            strengthSlider = viewController.AddSlider("Strength");
            strengthSlider.onValueChanged += delegate (float value) {
                var oldData = muscle.MuscleData;
                var strength = SliderToStrength(value);
                if (strength != oldData.strength) {
                    // FIXME: We only want to do this at the end of the drag!!
                    DataWillChange();
                }
                var data = new MuscleData(
                    oldData.id, oldData.startBoneID, oldData.endBoneID,
                    strength, oldData.canExpand
                );

                muscle.MuscleData = data;
                Refresh();
            };

            canExpandToggle = viewController.AddToggle("Can Expand");
            canExpandToggle.onValueChanged += delegate (bool canExpand) {
                var oldData = muscle.MuscleData;
                if (canExpand != oldData.canExpand) {
                    DataWillChange();
                }
                var data = new MuscleData(
                    oldData.id, oldData.startBoneID, oldData.endBoneID,
                    oldData.strength, canExpand
                );

                muscle.MuscleData = data;
                Refresh();
            };

            Refresh();
        }

        private void Refresh() {
            var visualStrength = muscle.MuscleData.strength / 1500f;
            strengthSlider.Refresh(StrengthToSlider(muscle.MuscleData.strength), string.Format("{0}x", visualStrength.ToString("0.0")));
            canExpandToggle.Refresh(muscle.MuscleData.canExpand);
        }

        private float SliderToStrength(float value) {
            return value * (MAX_STRENGTH - MIN_STRENGTH) + MIN_STRENGTH;
        }

        private float StrengthToSlider(float strength) {
            return (strength - MIN_STRENGTH) / (MAX_STRENGTH - MIN_STRENGTH);
        }
    }
}