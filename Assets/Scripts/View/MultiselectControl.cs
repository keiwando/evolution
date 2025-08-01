using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

namespace Keiwando.UI {

  public struct MultiselectControlItem {
    public string Name;
  }

  public class MultiselectControl: MonoBehaviour {

    public UnityEvent<int> onCurrentIndexChanged = new UnityEvent<int>();

    public MultiselectControlItem[] items;

    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button textCoverButton;
    [SerializeField] private TMP_Text label;

    public int CurrentIndex { 
      get { return currentIndex; }
      set {
        int itemCount = items != null ? items.Length : 0;
        int oldValue = currentIndex;
        currentIndex = Mathf.Clamp(value, 0, itemCount);
        if (oldValue != currentIndex && notifyAboutIndexChange) {
          onCurrentIndexChanged.Invoke(currentIndex);
        }
        Refresh();
      }
    }
    private int currentIndex = 0;
    private bool notifyAboutIndexChange = true;

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

    public void SetCurrentIndexWithoutNotify(int index) {
      notifyAboutIndexChange = false;
      CurrentIndex = index;
      notifyAboutIndexChange = true;
    }

    private void ChangeIndex(int offset) {
        CurrentIndex = ((currentIndex + offset + items.Length) % items.Length);
    }
  }
}