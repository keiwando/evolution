using Keiwando.Evolution.UI;
using Keiwando.UI;

namespace Keiwando.Evolution {

    public class BoneSettingsManager: BodyComponentSettingsManager {

        private const float MIN_WEIGHT = 0.5f;
        private const float MAX_WEIGHT = 5f;

        private Bone bone;
        private AdvancedBodyControlsViewController viewController;
        private LabelledSlider weightSlider;
        private LabelledToggle wingToggle;

        public BoneSettingsManager(Bone bone, AdvancedBodyControlsViewController viewController): base() {
            this.bone = bone;
            this.viewController = viewController;

            viewController.Reset();
            viewController.SetTitle("Bone Settings");
            weightSlider = viewController.AddSlider("Weight");
            weightSlider.onValueChanged += delegate (float value) {
                var oldData = bone.BoneData;
                var weight = SliderToWeight(value);
                if (weight != oldData.weight) {
                    // FIXME: We only want to do this at the end of the drag!!
                    DataWillChange();
                }
                var data = new BoneData(
                    oldData.id, oldData.startJointID, oldData.endJointID,
                    weight, oldData.isWing
                );
                bone.BoneData = data;
                Refresh();
            };

            // DEBUG:
            wingToggle = viewController.AddToggle("Wing");
            wingToggle.onValueChanged += delegate (bool isWing) {
                var oldData = bone.BoneData;
                if (isWing != oldData.isWing) {
                    DataWillChange();
                }
                var data = new BoneData(
                    oldData.id, oldData.startJointID, oldData.endJointID,
                    oldData.weight, isWing
                );
                bone.BoneData = data;
                Refresh();
            };

            Refresh();
        }

        private void Refresh() {
            var weight = bone.BoneData.weight;
            weightSlider.Refresh(WeightToSlider(weight), string.Format("{0}x", weight.ToString("0.0")));
            wingToggle.Refresh(bone.BoneData.isWing);
        }

        private float SliderToWeight(float value) {
            return value * (MAX_WEIGHT - MIN_WEIGHT) + MIN_WEIGHT;
        }

        private float WeightToSlider(float weight) {
            return (weight - MIN_WEIGHT) / (MAX_WEIGHT - MIN_WEIGHT);
        }
    }
}