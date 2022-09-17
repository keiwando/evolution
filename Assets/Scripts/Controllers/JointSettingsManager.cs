using Keiwando.Evolution.UI;
using Keiwando.UI;

namespace Keiwando.Evolution {

    public class JointSettingsManager: BodyComponentSettingsManager {

        private const string PENALTY_TOOLTIP = "The fitness of the creature is reduced by this amount if this joint ever touches the ground.";

        private const float MIN_WEIGHT = 0.2f;
        private const float MAX_WEIGHT = 5f;

        private Joint joint;
        private AdvancedBodyControlsViewController viewController;
        private LabelledSlider weightSlider;
        private LabelledSlider penaltySlider;

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
                var data = new JointData(oldData.id, oldData.position, weight, oldData.fitnessPenaltyForTouchingGround);
                joint.JointData = data;
                Refresh();
            };
            
            penaltySlider = viewController.AddSlider("Fitness Penalty", new TooltipData(PENALTY_TOOLTIP, 90f));
            penaltySlider.onDragWillBegin += delegate () {
                DataWillChange();
            };
            penaltySlider.onValueChanged += delegate (float penalty) {
                var oldData = joint.JointData;
                var data = new JointData(oldData.id, oldData.position, oldData.weight, penalty);
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
        }

        private float SliderToWeight(float value) {
            return value * (MAX_WEIGHT - MIN_WEIGHT) + MIN_WEIGHT;
        }

        private float WeightToSlider(float weight) {
            return (weight - MIN_WEIGHT) / (MAX_WEIGHT - MIN_WEIGHT);
        }
    }
}