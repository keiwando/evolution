using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Keiwando.Evolution.UI {

    public struct HelpPage {
        public string Title { get; set; }
        public string Text { get; set; }
    }

    public struct HelpPages {
        public TMP_FontAsset Font { get; set; }
        public HelpPage[] Pages { get; set; }
    }

    public class HelpPagesView: MonoBehaviour {

        private const float DEFAULT_BUTTON_ALPHA = 0.5f;
        private const float SELECTED_BUTTON_ALPHA = 1f;

        [SerializeField] private GridLayoutGroup sectionList;
        [SerializeField] private Button sectionButtonTemplate;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private TMP_Text textContainerTemplate;

        private TMP_Text[] sectionTextContainers;
        private Button[] sectionButtons;
        private CanvasGroup[] buttonCanvasGroups;

        private int currentSection = 0;

        public void Setup(HelpPages pages) {

            // Destroy all old items
            if (sectionButtons != null) {
                for (int i = 0; i < sectionButtons.Length; i++) {
                    Destroy(sectionButtons[i].gameObject);
                    Destroy(sectionTextContainers[i].gameObject);
                }
            }

            // Create the new containers and setup their content
            sectionTextContainers = new TMP_Text[pages.Pages.Length];
            sectionButtons = new Button[pages.Pages.Length];
            buttonCanvasGroups = new CanvasGroup[sectionButtons.Length];

            for (int i = 0; i < pages.Pages.Length; i++) {
                var page = pages.Pages[i];
                var container = Instantiate(
                    textContainerTemplate, textContainerTemplate.transform.parent
                );
                var button = Instantiate(
                    sectionButtonTemplate, sectionList.transform
                );
                var buttonLabel = button.GetComponentInChildren<TMP_Text>();
                var buttonCanvasGroup = button.GetComponent<CanvasGroup>();
                buttonCanvasGroup.alpha = DEFAULT_BUTTON_ALPHA;
                // Set the font
                buttonLabel.font = pages.Font;
                container.font = pages.Font;

                int section = i;
                button.onClick.AddListener(delegate () {
                    ShowSection(section);
                });

                container.text = page.Text;
                buttonLabel.text = page.Title;

                sectionTextContainers[i] = container;
                sectionButtons[i] = button;
                buttonCanvasGroups[i] = buttonCanvasGroup;

                container.gameObject.SetActive(false);
            }

            sectionButtonTemplate.gameObject.SetActive(false);
            textContainerTemplate.gameObject.SetActive(false);

            ShowSection(0);
        }

        private void ShowSection(int i) {
            buttonCanvasGroups[currentSection].alpha = DEFAULT_BUTTON_ALPHA;
            sectionTextContainers[currentSection].gameObject.SetActive(false);
            currentSection = i;
            sectionTextContainers[i].gameObject.SetActive(true);
            buttonCanvasGroups[i].alpha = SELECTED_BUTTON_ALPHA;
            var sectionRect = sectionTextContainers[i].GetComponent<RectTransform>();
            scrollRect.content = sectionRect;
            var newPosition = sectionRect.localPosition;
            newPosition.y = 0;
            sectionRect.localPosition = newPosition;
        }
    }
}