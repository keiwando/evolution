using Keiwando.Evolution.UI;
using Keiwando.UI;

namespace Keiwando.Evolution {

    public class JointSettingsManager: BodyComponentSettingsManager {

        private const string PENALTY_TOOLTIP = "The fitness of the creature is reduced by this amount if this joint ever touches the ground.";
        private const string GOOGLY_EYE_TOOLTIP = "Displays the Joint as a Googly Eye.";

        private const float MIN_WEIGHT = 0.2f;
        private const float MAX_WEIGHT = 5f;

        private Joint joint;
        private AdvancedBodyControlsViewController viewController;
        private LabelledSlider weightSlider;
        private LabelledSlider penaltySlider;
        private LabelledToggle googlyEyeToggle;

        public JointSettingsManager(Joint joint, AdvancedBodyControlsViewController viewController): base() {
            this.joint = joint;
            this.viewController = viewController;

            viewController.Reset();
            viewController.SetTitle("Joint Settings");
            
            weightSlider = viewController.AddSlider("Weight");
            weightSlider.onDragWillBegin += delegate () {
                DataWillChange();
            };
            weightSlider.onValueChanged += delegate (float value) {
                var oldData = joint.JointData;
                var weight = SliderToWeight(value);
                var data = new JointData(oldData.id, oldData.position, weight, oldData.fitnessPenaltyForTouchingGround, oldData.isGooglyEye);
                joint.JointData = data;
                Refresh();
            };
            
            penaltySlider = viewController.AddSlider("Fitness Penalty", new TooltipData(PENALTY_TOOLTIP, 90f));
            penaltySlider.onDragWillBegin += delegate () {
                DataWillChange();
            };
            penaltySlider.onValueChanged += delegate (float penalty) {
                var oldData = joint.JointData;
                var data = new JointData(oldData.id, oldData.position, oldData.weight, penalty, oldData.isGooglyEye);
                joint.JointData = data;
                Refresh();
            };

            googlyEyeToggle = viewController.AddToggle("Googly Eye", new TooltipData(GOOGLY_EYE_TOOLTIP, 60f));
            googlyEyeToggle.onValueChanged += delegate (bool isGooglyEye) {
                var oldData = joint.JointData;
                if (isGooglyEye != oldData.isGooglyEye) {
                    DataWillChange();
                }
                var data = new JointData(oldData.id, oldData.position, oldData.weight, oldData.fitnessPenaltyForTouchingGround, isGooglyEye);
                joint.JointData = data;
                Refresh();
            };

            Refresh();
        }

        public override void Refresh() {
            var weight = joint.JointData.weight;
            weightSlider.Refresh(WeightToSlider(weight), string.Format("{0}x", weight.ToString("0.0")));
            var penalty = joint.JointData.fitnessPenaltyForTouchingGround;
            penaltySlider.Refresh(penalty, string.Format("{0}%", (int)(penalty * 100.0f)));
            googlyEyeToggle.Refresh(joint.JointData.isGooglyEye);
        }

        private float SliderToWeight(float value) {
            return value * (MAX_WEIGHT - MIN_WEIGHT) + MIN_WEIGHT;
        }

        private float WeightToSlider(float weight) {
            return (weight - MIN_WEIGHT) / (MAX_WEIGHT - MIN_WEIGHT);
        }
    }
}