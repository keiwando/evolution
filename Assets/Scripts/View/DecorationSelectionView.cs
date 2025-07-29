using System;
using UnityEngine;
using UnityEngine.UI;

namespace Keiwando.Evolution.UI {

  public class DecorationSelectionView : MonoBehaviour {

    public CreatureEditor creatureEditor;

    [SerializeField] private GridLayoutGroup grid;
    [SerializeField] private DecorationCell cellTemplate;
    private DecorationCell[] cells;

    void Start() {
      
      cells = new DecorationCell[orderedDecorationTypesForGrid.Length];
      for (int i = 0; i < orderedDecorationTypesForGrid.Length; i++) {
        DecorationType decorationType = orderedDecorationTypesForGrid[i];
        // Have to copy in order not to accidentally capture the `i` reference in the button delegate
        int decorationIndex = i;
        DecorationCell cell = Instantiate(cellTemplate.gameObject, grid.transform).GetComponent<DecorationCell>();
        cell.iconImage.sprite = DecorationUtils.DecorationTypeToImageResourceName(decorationType);
        cell.selectionHighlight.gameObject.SetActive(false);
        Button cellButton = cell.GetComponent<Button>();
        cellButton.onClick.AddListener(delegate () {
          OnCellIndexSelected(decorationIndex);
        });
        this.cells[i] = cell;
      }
      cellTemplate.gameObject.SetActive(false);

      OnCellIndexSelected(FindCellIndexOfDecorationType(creatureEditor.SelectedDecorationType));
    }

    private void OnCellIndexSelected(int cellIndex) {
      if (cellIndex < 0 || cellIndex >= orderedDecorationTypesForGrid.Length) {
        return;
      }
      int previouslySelectedCellIndex = FindCellIndexOfDecorationType(creatureEditor.SelectedDecorationType);
      creatureEditor.SelectedDecorationType = orderedDecorationTypesForGrid[cellIndex];

      if (previouslySelectedCellIndex >= 0) {
        DecorationCell previouslySelectedCell = cells[previouslySelectedCellIndex];
        previouslySelectedCell.selectionHighlight.gameObject.SetActive(false);
      }
      DecorationCell cell = cells[cellIndex];
      cell.selectionHighlight.gameObject.SetActive(true);
    }

    private int FindCellIndexOfDecorationType(DecorationType type) {
      for (int i = 0; i < orderedDecorationTypesForGrid.Length; i++) {
        if (orderedDecorationTypesForGrid[i] == type) { 
          return i;
        }
      }
      return -1;
    }

    private static DecorationType[] orderedDecorationTypesForGrid = {
      DecorationType.GooglyEye,
      DecorationType.ShiftyEyes,
      DecorationType.EmptyEyeOval,
      DecorationType.EmptyEyeCircle,
      DecorationType.EmptyEyeRounded,
      DecorationType.EmptyEyeSlanted,
      DecorationType.EmptyEyeHappy,
      DecorationType.PupilDot,
      DecorationType.PupilTriangleCut,
      DecorationType.PupilReflection,
      DecorationType.EyebrowNormal,
      DecorationType.EyebrowRaised,
      DecorationType.EyebrowAngry,
      DecorationType.MouthSmile,
      DecorationType.MouthLaugh,
      DecorationType.MouthLipSmile,
      DecorationType.MouthWorried,
      DecorationType.MouthLips,
      DecorationType.MouthLipsSmall,
      DecorationType.MouthTongue,
      DecorationType.NoseBigFront,
      DecorationType.NoseRaised,
      DecorationType.NoseSmall,
      DecorationType.NoseCrooked,
      DecorationType.NoseDroopy,
      DecorationType.Moustache1,
      DecorationType.Moustache2,
      DecorationType.Moustache3,
      DecorationType.EarSide,
      DecorationType.EarFront,
      DecorationType.HandOpenPalm,
      DecorationType.HandBack,
      DecorationType.Fist,
      DecorationType.Foot,
      DecorationType.Shoe,
      DecorationType.ShoeHighHeel,
      DecorationType.ShoeCartoon,
      DecorationType.Brain,
      DecorationType.Bone,
      DecorationType.CatEar,
      DecorationType.CatWhiskers,
      DecorationType.CatSnout
    };
  }
}