using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Keiwando.UI;
using TMPro;

namespace Keiwando.Evolution.UI {

    public class AdvancedBodyControlsViewController: MonoBehaviour {

        [SerializeField] private TMP_Text title;
        
        [SerializeField] private LabelledSlider sliderTemplate;
        [SerializeField] private LabelledToggle toggleTemplate;

        // Note: We have to use a VerticalLayoutGroup instead of a GridLayout because
        // the GridLayout has a fixed size for all children and we need the toggles to be
        // smaller than the slider cell height.
        // The VerticalLayoutGroup does not provide a spacing option, so we bake the trailing spacing
        // into each cell.
        [SerializeField] private VerticalLayoutGroup grid;

        private List<GameObject> listItems = new List<GameObject>();

        void Start() {
            sliderTemplate.gameObject.SetActive(false);
            toggleTemplate.gameObject.SetActive(false);
        }

        public LabelledSlider AddSlider(string title, TooltipData tooltip = null) {
            
            var slider = Instantiate(sliderTemplate, grid.transform);
            listItems.Add(slider.gameObject);
            slider.gameObject.SetActive(true);
            slider.Description = title;
            slider.TooltipData = tooltip;
            return slider;
        }

        public LabelledToggle AddToggle(string title, TooltipData tooltip = null) {
            
            var toggle = Instantiate(toggleTemplate, grid.transform);
            listItems.Add(toggle.gameObject);
            toggle.gameObject.SetActive(true);
            toggle.Description = title;
            toggle.TooltipData = tooltip;
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