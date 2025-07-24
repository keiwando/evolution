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
      DecorationType.Emoji_Eyes,
      DecorationType.Emoji_Smile,
      DecorationType.Emoji_Neutral_Face,
      DecorationType.Emoji_No_Mouth,
      DecorationType.Emoji_Grimacing,
      DecorationType.Emoji_Sunglasses_Face,
      DecorationType.Emoji_Skull,
      DecorationType.Emoji_Clown,
      DecorationType.Emoji_Alien,
      DecorationType.Emoji_Robot,
      DecorationType.Emoji_Waving,
      DecorationType.Emoji_Hand,
      DecorationType.Emoji_Peace,
      DecorationType.Emoji_Horns,
      DecorationType.Emoji_Call_Me,
      DecorationType.Emoji_Leg,
      DecorationType.Emoji_Foot,
      DecorationType.Emoji_Nose,
      DecorationType.Emoji_Brain,
      DecorationType.Emoji_Eye,
      DecorationType.Emoji_Mouth,
      DecorationType.Emoji_Dog,
      DecorationType.Emoji_Cat,
      DecorationType.Emoji_Unicorn,
      DecorationType.Emoji_Wheel,
      DecorationType.Emoji_Shoe1,
      DecorationType.Emoji_Shoe2,
      DecorationType.Emoji_Shoe3,
      DecorationType.Emoji_Shoe4,
      DecorationType.Emoji_Shoe5,
      DecorationType.Emoji_Shoe6,
      DecorationType.Emoji_Shoe7,
      DecorationType.Emoji_Hat,
      DecorationType.Emoji_Crown,
      DecorationType.Emoji_Top_Hat,
      DecorationType.Emoji_Saxophone,
      DecorationType.Emoji_Guitar,
      DecorationType.Emoji_Bone
    };
  }
}