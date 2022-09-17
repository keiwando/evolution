using UnityEngine;
using UnityEngine.UI;

namespace Keiwando.Evolution.UI {

    public class TooltipData {
        public readonly string text;
        public readonly float height;

        public TooltipData(string text = "", float height = 70.0f) {
            this.text = text; this.height = height;
        }
    }

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

            tooltip.SetActive(false);
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

        void OnDestroy() {
            // Make sure that the tooltip gets cleaned up properly even if it is parented to something else 
            // while it's being shown.
            if (tooltip != null && tooltip.transform.parent != previousParent) {
                Destroy(tooltip);
            }
        }

        private void ShowTooltip() {
            tooltip.transform.SetParent(rootContainer, true);
            tooltip.transform.SetAsLastSibling();
            tooltip.SetActive(true);
        }

        private void HideTooltip() {
            tooltip.SetActive(false);
            tooltip.transform.SetParent(previousParent, true);
        }

        public void SetData(TooltipData data) {
            tooltipText.text = data.text;
            (tooltip.transform as RectTransform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, data.height);
        }
    }
}