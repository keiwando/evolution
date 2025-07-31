using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Keiwando.UI {

  public class SettingsTab: MonoBehaviour {

    private static Color selectedBackgroundColor = Color.white;
    private static Color defaultBackgroundColor = new Color(0.21f, 0.21f, 0.21f);
    private static Color selectedTextColor = new Color(0.2f, 0.2f, 0.2f);
    private static Color defaultTextColor = new Color(0.86f, 0.86f, 0.86f);

    public TMP_Text label;
    public Image backgroundImage;

    public void SetSelected(bool selected) {
      label.color = selected ? selectedTextColor : defaultTextColor;
      backgroundImage.color = selected ? selectedBackgroundColor : defaultBackgroundColor;
    }
  }
}