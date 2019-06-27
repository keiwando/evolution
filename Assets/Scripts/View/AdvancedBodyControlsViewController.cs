using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Keiwando.UI;

namespace Keiwando.Evolution.UI {

    public class AdvancedBodyControlsViewController: MonoBehaviour {

        [SerializeField] private Text title;
        
        [SerializeField] private LabelledSlider sliderTemplate;
        [SerializeField] private LabelledToggle toggleTemplate;

        private List<GameObject> listItems = new List<GameObject>();

        void Start() {
            sliderTemplate.gameObject.SetActive(false);
            toggleTemplate.gameObject.SetActive(false);
        }

        public LabelledSlider AddSlider(string title) {
            
            var slider = Instantiate(sliderTemplate, sliderTemplate.transform.position, Quaternion.identity, transform);
            listItems.Add(slider.gameObject);
            slider.gameObject.SetActive(true);
            slider.Description = title;
            return slider;
        }

        public LabelledToggle AddToggle(string title) {
            
            var toggle = Instantiate(toggleTemplate);
            toggle.gameObject.SetActive(true);
            toggle.Description = title;
            return toggle;
        }

        public void SetTitle(string title) {
            this.title.text = title;
        }

        public void Reset() {
            foreach (var item in listItems) {
                Destroy(item);
            }
            listItems.Clear();
        }
    }
}