using UnityEngine;
using UnityEngine.UI;

namespace Keiwando.Evolution.UI {

    [RequireComponent(typeof(Button))]
    public class ClickableTooltip: MonoBehaviour {

        /// <summary>
        /// The container which the tooltip gameobject needs to be parented
        /// to, so that it renders after everything else inside of that 
        /// container.
        /// </summary>
        [SerializeField] private Transform rootContainer;

        [SerializeField] private GameObject tooltip;
        [SerializeField] private Text tooltipText;

        private bool clickedThisFrame = false;  
        private Transform previousParent;

        void Start() {

            if (tooltip == null) {
                tooltip = transform.GetChild(0).gameObject;
            }

            var button = GetComponent<Button>();
            button.onClick.AddListener(delegate () {
                clickedThisFrame = true;
                if (tooltip.activeSelf) {
                    HideTooltip();
                } else {
                    ShowTooltip();
                }
            });

            tooltip.gameObject.SetActive(false);
            previousParent = tooltip.transform.parent;
        }

        void Update() {

            // Hide the tooltip with a click
            if (tooltip.activeSelf) {
                if (!clickedThisFrame && Input.GetMouseButtonUp(0)) {
                    HideTooltip();
                } else {
                    clickedThisFrame = false;
                }
            }
        }

        private void ShowTooltip() {
            tooltip.transform.SetParent(rootContainer, true);
            tooltip.transform.SetAsLastSibling();
            tooltip.gameObject.SetActive(true);
        }

        private void HideTooltip() {
            tooltip.SetActive(false);
            tooltip.transform.SetParent(previousParent, true);
        }

        public void SetText(string text) {
            // DEBUG:
            Debug.Log("Setting tooltip text to " + text);
            tooltipText.text = text;
        }
    }
}