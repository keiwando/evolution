using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Keiwando.UI {

  public struct MultiselectControlItem {
    public string Name;
  }

  public class MultiselectControl: MonoBehaviour {

    public MultiselectControlItem[] items;

    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button textCoverButton;
    [SerializeField] private TMP_Text label;

    public int CurrentIndex { 
      get { return currentIndex; }
      set {
        int itemCount = items != null ? items.Length : 0;
        currentIndex = Mathf.Clamp(value, 0, itemCount);
        Refresh();
      }
    }
    private int currentIndex = 0;

    void Start() {

      previousButton.onClick.AddListener(delegate () {
        ChangeIndex(-1);
      });
      nextButton.onClick.AddListener(delegate () {
        ChangeIndex(1);
      });
      textCoverButton.onClick.AddListener(delegate () {
        ChangeIndex(1);
      });

      Refresh();
    }

    public void Refresh() {
      if (items != null && currentIndex >= 0 && currentIndex < items.Length) {
        label.SetText(items[currentIndex].Name);
      } else {
        label.SetText("");
      }
    }

    private void ChangeIndex(int offset) {
        CurrentIndex = ((currentIndex + offset + items.Length) % items.Length);
    }
  }
}