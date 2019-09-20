using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
 
namespace Keiwando.UI {

    public class ListLayoutGroup : LayoutGroup {

        [SerializeField]
        protected float cellHeight = 100f;
        public float CellHeight {
            get { return cellHeight; }
            set { SetProperty(ref cellHeight, value); }
        }

        [SerializeField]
        protected float spacing = 0;
        public float Spacing { 
            get { return spacing; } 
            set { SetProperty(ref spacing, value); } 
        }
 
        protected ListLayoutGroup() { }
 
#if UNITY_EDITOR
        protected override void OnValidate() {
            base.OnValidate();
        }
#endif
 
        public override void CalculateLayoutInputHorizontal() {
            base.CalculateLayoutInputHorizontal();
 
            float width = rectTransform.rect.size.x;
            SetLayoutInputForAxis(width, width, -1, 0);
        }
 
        public override void CalculateLayoutInputVertical() {
            
            float width = rectTransform.rect.size.x;
            float minSpace = padding.vertical + (cellHeight + spacing) * rectChildren.Count - spacing;
            SetLayoutInputForAxis(minSpace, minSpace, -1, 1);
        }
 
        public override void SetLayoutHorizontal() {
            SetCellsAlongAxis(0);
        }
 
        public override void SetLayoutVertical() {
            SetCellsAlongAxis(1);
        }
 
        private void SetCellsAlongAxis(int axis) {
 
            float width = rectTransform.rect.size.x;
            float height = rectTransform.rect.size.y;
 
            int cellCount = Mathf.Max(1, Mathf.FloorToInt((height - padding.vertical + spacing + 0.001f) / (cellHeight + spacing)));
            
            int actualCellCountY = Mathf.Clamp(cellCount, 1, rectChildren.Count);
 
            Vector2 requiredSpace = new Vector2(width, actualCellCountY * cellHeight + (actualCellCountY - 1) * spacing);
            Vector2 startOffset = new Vector2(GetStartOffset(0, requiredSpace.x),
                                              GetStartOffset(1, requiredSpace.y));
 
            for (int i = 0; i < rectChildren.Count; i++) {

                int positionX;
                int positionY;
                positionX = i / cellCount;
                positionY = i % cellCount;
 
                SetChildAlongAxis(rectChildren[i], 0, startOffset.x + width * positionX, width);
                SetChildAlongAxis(rectChildren[i], 1, startOffset.y + (cellHeight + spacing) * positionY, cellHeight);
            }
        }
    }
}