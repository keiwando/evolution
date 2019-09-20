using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Keiwando.UI;

namespace Keiwando.Evolution.UI {

    public interface IBasicSettingsViewDelegate {

        int GetPopulationSize();
        int GetGenerationDuration();
        Objective GetObjective();

        void PopulationSizeDidChange(int value);
        void SimulationTimeDidChange(int value);
        void ObjectiveDidChange(Objective objective);
    }

    public class BasicSettingsView: MonoBehaviour {

        public IBasicSettingsViewDelegate Delegate { get; set; }

        [SerializeField] private InputField populationSizeInput;
        [SerializeField] private InputField generationDurationInput;
        [SerializeField] private Dropdown taskDropdown;

        private Dropdown<int> dropdownWrapper;

        void Start() {

            var dropdownData = new List<Dropdown<int>.Data> {
                new Dropdown<int>.Data() {
                    Value = (int)Objective.Running,
                    Label = "Running"
                },
                new Dropdown<int>.Data() {
                    Value = (int)Objective.Jumping,
                    Label = "Jumping"
                },
                new Dropdown<int>.Data() {
                    Value = (int)Objective.ObstacleJump,
                    Label = "Obstacle Jump"
                },
                new Dropdown<int>.Data() {
                    Value = (int)Objective.Climbing,
                    Label = "Climbing"
                }
            };
            
            var taskDropdown = new Dropdown<int>(this.taskDropdown, dropdownData);
            taskDropdown.onValueChanged += delegate (int value) {
                var objective = (Objective)value;
                Delegate?.ObjectiveDidChange(objective);
            };
            this.dropdownWrapper = taskDropdown;

            populationSizeInput.onEndEdit.AddListener(delegate (string value) {
                int populationSize = 0;
                if (int.TryParse(value, out populationSize)) {
                    Delegate?.PopulationSizeDidChange(populationSize);
                }
                Refresh();
            });

            generationDurationInput.onEndEdit.AddListener(delegate (string value) {
                int duration = 0;
                if (int.TryParse(value, out duration)) {
                    Delegate?.SimulationTimeDidChange(duration);
                }
                Refresh();
            });
        }

        public void Refresh() {

            if (Delegate == null) return;

            populationSizeInput.text = Delegate.GetPopulationSize().ToString();
            generationDurationInput.text = Delegate.GetGenerationDuration().ToString();
            this.taskDropdown.value = (int)Delegate.GetObjective();
        }
    }
}