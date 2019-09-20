using UnityEngine;
using UnityEngine.UI;
#if UNITY_WEBGL
using System.Runtime.InteropServices;
using UnityEngine.EventSystems;
#endif

namespace Keiwando.UI {

    [RequireComponent(typeof(Button))]
    public class WebLink: MonoBehaviour
    #if UNITY_WEBGL
    , IPointerDownHandler
    #endif
     {

        #if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void _openLink(string url);
        
        [DllImport("__Internal")]
        private static extern void _openLinkOnMouseUp(string url);
        #endif

        [SerializeField]
        private string url;

        void Start() {
            var button = GetComponent<Button>();
            button.onClick.AddListener(delegate () {
                OpenLink();
            });
        }

        private void OpenLink() {
            #if UNITY_WEBGL 
            // _openLink(url);
            #else 
            Application.OpenURL(url);
            #endif
        }

        #if UNITY_WEBGL
        public void OnPointerDown(PointerEventData eventData) {
            _openLinkOnMouseUp(url);
        }
        #endif
    }
}