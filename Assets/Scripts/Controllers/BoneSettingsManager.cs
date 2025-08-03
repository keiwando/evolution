using System.Collections.Generic;
using UnityEngine;
using Keiwando.Evolution.UI;
using Keiwando.UI;

namespace Keiwando.Evolution {

    public class BoneSettingsManager: BodyComponentSettingsManager {

        private const string WING_TOOLTIP = "These wings generate force when they are moved in the direction of their feathers.\nTip: Reduce the weight of wing bones and increase their connected muscle strengths for better results.";
        private const string INVERT_TOOLTIP = "Flips the wing and therefore the direction of the force that it generates.";

        private const float MIN_WEIGHT = 0.5f;
        private const float MAX_WEIGHT = 5f;

        private Bone bone;
        private List<Decoration> decorations;
        private AdvancedBodyControlsViewController viewController;
        private LabelledSlider weightSlider;
        private LabelledToggle wingToggle;
        private LabelledToggle invertedToggle;

        public BoneSettingsManager(Bone bone, List<Decoration> decorations, AdvancedBodyControlsViewController viewController): base() {
            this.bone = bone;
            this.decorations = decorations;
            this.viewController = viewController;

            viewController.Reset();
            viewController.SetTitle("Bone Settings");
            weightSlider = viewController.AddSlider("Weight");
            weightSlider.onDragWillBegin += delegate () {
                DataWillChange();
            };
            weightSlider.onValueChanged += delegate (float value) {
                var oldData = bone.BoneData;
                var weight = SliderToWeight(value);
                var data = new BoneData(
                    oldData.id, oldData.startJointID, oldData.endJointID,
                    weight, oldData.isWing, oldData.inverted
                );
                bone.BoneData = data;
                Refresh();
            };

            wingToggle = viewController.AddToggle("Wing", new TooltipData(WING_TOOLTIP, 146f));
            wingToggle.onValueChanged += delegate (bool isWing) {
                var oldData = bone.BoneData;
                if (isWing != oldData.isWing) {
                    DataWillChange();
                }
                var data = new BoneData(
                    oldData.id, oldData.startJointID, oldData.endJointID,
                    oldData.weight, isWing, oldData.inverted
                );
                bone.BoneData = data;
                Refresh();
            };

            invertedToggle = viewController.AddToggle("Invert", new TooltipData(INVERT_TOOLTIP));
            invertedToggle.onValueChanged += delegate (bool inverted) {
                var oldData = bone.BoneData;
                if (inverted != oldData.inverted) {
                    DataWillChange();
                }
                var data = new BoneData(
                    oldData.id, oldData.startJointID, oldData.endJointID,
                    oldData.weight, oldData.isWing, inverted
                );
                bone.BoneData = data;
                foreach (Decoration decoration in this.decorations) {
                    if (decoration.DecorationData.boneId == bone.BoneData.id) {
                        var oldPosition = decoration.transform.position;
                        var targetLocalPosition = bone.transform.InverseTransformPoint(oldPosition);
                        var decorationData = decoration.DecorationData;
                        decorationData.offset = new Vector2(targetLocalPosition.x, targetLocalPosition.y);
                        decoration.DecorationData = decorationData;
                        decoration.UpdateOrientation();
                    }
                }
                Refresh();
            };

            Refresh();
        }

        public override void Refresh() {
            var weight = bone.BoneData.weight;
            weightSlider.Refresh(WeightToSlider(weight), string.Format("{0}x", weight.ToString("0.0")));
            wingToggle.Refresh(bone.BoneData.isWing);
            invertedToggle.Refresh(bone.BoneData.inverted);
        }

        private float SliderToWeight(float value) {
            return value * (MAX_WEIGHT - MIN_WEIGHT) + MIN_WEIGHT;
        }

        private float WeightToSlider(float weight) {
            return (weight - MIN_WEIGHT) / (MAX_WEIGHT - MIN_WEIGHT);
        }
    }
}