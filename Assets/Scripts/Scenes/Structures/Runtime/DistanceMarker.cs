using UnityEngine;

namespace Keiwando.Evolution.Scenes {

    public class DistanceMarker: MonoBehaviour {

        public string Text {
            get => text?.text ?? "";
            set {
                if (text != null) {
                    text.text = value;
                }
            }
        }

        public Color TextColor {
            set {
                if (text != null)
                    text.color = value;
            }
        }

        [SerializeField]
        private TextMesh text;
    }
}