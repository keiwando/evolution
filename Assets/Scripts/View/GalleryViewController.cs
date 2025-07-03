using UnityEngine;
using UnityEngine.UI;
using Keiwando;
using Keiwando.UI;

namespace Keiwando.Evolution.UI {

  public class GalleryViewController: MonoBehaviour {

    [SerializeField]
    private CustomGridLayoutGroup grid;

    [SerializeField]
    private GalleryGridCell templateGridCell;

    [SerializeField]
    private Button closeButton;

    void Start() {

      int numberOfItemsPerPage = grid.ColumnCount * grid.RowCount;

      for (int i = 0; i < numberOfItemsPerPage; i++) {
        var cell = Instantiate(templateGridCell, grid.transform);
        cell.gameObject.SetActive(true);
        // Here you would typically set the image for the cell.
        // Example: cell.rawImage.texture = ...; (load texture from a source)
      }
      templateGridCell.gameObject.SetActive(false);

      closeButton.onClick.AddListener(delegate () {
          Hide();
      });
    }

    public void Refresh() {

    }
    
    public void Show() {
        gameObject.SetActive(true);
        InputRegistry.shared.Register(InputType.AndroidBack, this);
        GestureRecognizerCollection.shared.GetAndroidBackButtonGestureRecognizer().OnGesture += OnAndroidBack;
        Refresh();
    }

    public void Hide() {
        InputRegistry.shared.Deregister(this);
        GestureRecognizerCollection.shared.GetAndroidBackButtonGestureRecognizer().OnGesture -= OnAndroidBack;
        gameObject.SetActive(false);
    }

    private void OnAndroidBack(AndroidBackButtonGestureRecognizer rec) {
        if (InputRegistry.shared.MayHandle(InputType.AndroidBack, this))
            Hide();
    }
  }

}