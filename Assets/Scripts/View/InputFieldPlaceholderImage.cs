using UnityEngine;
using UnityEngine.UI;

namespace Keiwando.UI {

    [RequireComponent(typeof(TMPro.TMP_InputField))]
    public class InputFieldPlaceholderImage: MonoBehaviour {
        
        [SerializeField]
        private Image placeholder;

        void Start() {
            var inputField = GetComponent<TMPro.TMP_InputField>();
            inputField.onValueChanged.AddListener(delegate (string value) {
                placeholder.gameObject.SetActive(string.IsNullOrEmpty(value));
            });
        }
    }
}