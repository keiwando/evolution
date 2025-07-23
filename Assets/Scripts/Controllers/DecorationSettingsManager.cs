using Keiwando.Evolution.UI;
using Keiwando.UI;

namespace Keiwando.Evolution {

  public class DecorationSettingsManager: BodyComponentSettingsManager {

    private Decoration decoration;
    private AdvancedBodyControlsViewController viewController;
    private LabelledToggle flipXToggle;
    private LabelledToggle flipYToggle;

    public DecorationSettingsManager(Decoration decoration, AdvancedBodyControlsViewController viewController): base() {
      this.decoration = decoration;
      this.viewController = viewController;

      viewController.Reset();
      viewController.SetTitle("Decoration Settings");

      flipXToggle = viewController.AddToggle("Flip Horizontal");
      flipXToggle.onValueChanged += delegate (bool flipX) {
        var oldData = decoration.DecorationData;
        if (flipX != oldData.flipX) {
          DataWillChange();
        }
        var data = oldData;
        data.flipX = flipX;
        decoration.DecorationData = data;
        Refresh();
      };
      
      flipYToggle = viewController.AddToggle("Flip Vertical");
      flipYToggle.onValueChanged += delegate (bool flipY) {
        var oldData = decoration.DecorationData;
        if (flipY != oldData.flipY) {
          DataWillChange();
        }
        var data = oldData;
        data.flipY = flipY;
        decoration.DecorationData = data;
        Refresh();
      };
    }

    public override void Refresh() {
      flipXToggle.Refresh(decoration.DecorationData.flipX);
      flipYToggle.Refresh(decoration.DecorationData.flipY);
    }
  }
}