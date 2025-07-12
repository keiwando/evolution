using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Keiwando;

public class BasicTextInfoView: MonoBehaviour {

  [SerializeField]
  private string assetFolderName;

  [SerializeField]
  private TMP_Text textArea;
  [SerializeField]
  private Button closeButton;

  void Start() {
    string text = Resources.Load<TextAsset>($"Text/{assetFolderName}/en").text;
    textArea.SetText(text);

    closeButton.onClick.AddListener(delegate () {
        Close();
    });
  }

  public void Show(bool hideToggle = false) {

    InputRegistry.shared.Register(InputType.AndroidBack, this);
    GestureRecognizerCollection.shared.GetAndroidBackButtonGestureRecognizer().OnGesture += OnAndroidBack;
    this.gameObject.SetActive(true);
    Time.timeScale = 0;
  }

  public void Close() {
    Time.timeScale = 1;
    InputRegistry.shared.Deregister(this);
    GestureRecognizerCollection.shared.GetAndroidBackButtonGestureRecognizer().OnGesture -= OnAndroidBack;
    this.gameObject.SetActive(false);
  }

  private void OnAndroidBack(AndroidBackButtonGestureRecognizer rec) {
    if (InputRegistry.shared.MayHandle(InputType.AndroidBack, this)) {
      Close();
    }
  }
}