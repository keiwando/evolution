using Keiwando.Evolution.UI;
using Keiwando.UI;

namespace Keiwando.Evolution {

    public class MuscleSettingsManager: BodyComponentSettingsManager {

        private const string CAN_EXPAND_TOOLTIP = "Whether the muscle should be able to expand or only contract.";
        // FIXME: Write a proper tooltip here
        private const string USER_ID_TOOLTIP = "TEst";

        private const float MIN_STRENGTH = 0f;
        private const float MAX_STRENGTH = 4500f;

        private Muscle muscle;
        private AdvancedBodyControlsViewController viewController;
        private LabelledSlider strengthSlider;
        private LabelledToggle canExpandToggle;
        private LabelledInput userIdInput;

        public MuscleSettingsManager(Muscle muscle, AdvancedBodyControlsViewController viewController): base() {
            this.muscle = muscle;
            this.viewController = viewController;

            viewController.Reset();
            viewController.SetTitle("Muscle Settings");
            strengthSlider = viewController.AddSlider("Strength");
            strengthSlider.onDragWillBegin += delegate () {
                DataWillChange();
            };
            strengthSlider.onValueChanged += delegate (float value) {
                var oldData = muscle.MuscleData;
                var strength = SliderToStrength(value);
                var data = new MuscleData(
                    oldData.id, oldData.startBoneID, oldData.endBoneID,
                    strength, oldData.canExpand, oldData.userId
                );

                muscle.MuscleData = data;
                Refresh();
            };

            canExpandToggle = viewController.AddToggle("Can Expand", new TooltipData(CAN_EXPAND_TOOLTIP));
            canExpandToggle.onValueChanged += delegate (bool canExpand) {
                var oldData = muscle.MuscleData;
                if (canExpand != oldData.canExpand) {
                    DataWillChange();
                }
                var data = new MuscleData(
                    oldData.id, oldData.startBoneID, oldData.endBoneID,
                    oldData.strength, canExpand, oldData.userId
                );

                muscle.MuscleData = data;
                Refresh();
            };

            userIdInput = viewController.AddInput("Id", new TooltipData(USER_ID_TOOLTIP));
            userIdInput.onValueChanged += delegate (string userId) {
                if (userId == null) { userId = ""; }
                var oldData = muscle.MuscleData;
                DataWillChange();
                var data = new MuscleData(
                    oldData.id, oldData.startBoneID, oldData.endBoneID,
                    oldData.strength, oldData.canExpand, userId
                );

                muscle.MuscleData = data;
                Refresh();
            };

            Refresh();
        }

        public override void Refresh() {
            var visualStrength = muscle.MuscleData.strength / 1500f;
            strengthSlider.Refresh(StrengthToSlider(muscle.MuscleData.strength), string.Format("{0}x", visualStrength.ToString("0.0")));
            canExpandToggle.Refresh(muscle.MuscleData.canExpand);
            userIdInput.Refresh(muscle.MuscleData.userId);
        }

        private float SliderToStrength(float value) {
            return value * (MAX_STRENGTH - MIN_STRENGTH) + MIN_STRENGTH;
        }

        private float StrengthToSlider(float strength) {
            return (strength - MIN_STRENGTH) / (MAX_STRENGTH - MIN_STRENGTH);
        }
    }
}