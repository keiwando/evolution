using Keiwando.Evolution.UI;
using Keiwando.UI;

namespace Keiwando.Evolution {

    public class JointSettingsManager: BodyComponentSettingsManager {

        private const float MIN_WEIGHT = 0.2f;
        private const float MAX_WEIGHT = 5f;

        private Joint joint;
        private AdvancedBodyControlsViewController viewController;
        private LabelledSlider weightSlider;

        public JointSettingsManager(Joint joint, AdvancedBodyControlsViewController viewController): base() {
            this.joint = joint;
            this.viewController = viewController;

            viewController.Reset();
            viewController.SetTitle("Joint Settings");
            weightSlider = viewController.AddSlider("Weight");
            weightSlider.onValueChanged += delegate (float value) {
                var oldData = joint.JointData;
                var weight = SliderToWeight(value);
                if (weight != oldData.weight) {
                    DataWillChange();
                }
                var data = new JointData(oldData.id, oldData.position, weight);
                joint.JointData = data;
                Refresh();
            };
            Refresh();
        }

        private void Refresh() {
            var weight = joint.JointData.weight;
            weightSlider.Refresh(WeightToSlider(weight), string.Format("{0}x", weight.ToString("0.0")));
        }

        private float SliderToWeight(float value) {
            return value * (MAX_WEIGHT - MIN_WEIGHT) + MIN_WEIGHT;
        }

        private float WeightToSlider(float weight) {
            return (weight - MIN_WEIGHT) / (MAX_WEIGHT - MIN_WEIGHT);
        }
    }
}