using UnityEngine;
using UnityEngine.UI;

namespace Keiwando.UI {

    [RequireComponent(typeof(Button))]
    public class WebLink: MonoBehaviour {

        [SerializeField]
        private string url;

        void Start() {
            var button = GetComponent<Button>();
            button.onClick.AddListener(delegate () {
                OpenLink();
            });
        }

        private void OpenLink() {
            Application.OpenURL(url);
        }
    }
}