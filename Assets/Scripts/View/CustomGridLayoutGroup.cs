using UnityEngine;
using UnityEngine.UI;
 
namespace Keiwando.UI {

    public class CustomGridLayoutGroup : LayoutGroup {

        [SerializeField]
        protected int columnCount = 3;
        public int ColumnCount {
            get { return columnCount; }
            set { SetProperty(ref columnCount, value); }
        }
        
        [SerializeField]
        protected int rowCount = 3;
        public int RowCount {
            get { return rowCount; }
            set { SetProperty(ref rowCount, value); }
        }

        [SerializeField]
        protected float verticalSpacing = 0;
        public float VerticalSpacing { 
            get { return verticalSpacing; } 
            set { SetProperty(ref verticalSpacing, value); } 
        }

        [SerializeField]
        protected float horizontalSpacing = 0;
        public float HorizontalSpacing {
            get { return horizontalSpacing; } 
            set { SetProperty(ref horizontalSpacing, value); } 
        }

        protected CustomGridLayoutGroup() { }
 
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

            float height = rectTransform.rect.size.y;           
            SetLayoutInputForAxis(height, height, -1, 1);
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

            float cellWidth = (width - padding.horizontal - (columnCount - 1) * horizontalSpacing) / (float)columnCount;
            float cellHeight = (height - padding.vertical - (rowCount - 1) * verticalSpacing) / (float)rowCount;
            
            Vector2 requiredSpace = new Vector2(
              width - padding.horizontal, 
              height - padding.vertical
            );
            Vector2 startOffset = new Vector2(GetStartOffset(0, requiredSpace.x),
                                              GetStartOffset(1, requiredSpace.y));
 
            for (int i = 0; i < rectChildren.Count; i++) {
                if (axis == 0) {
                  int positionX = i % columnCount;
                  float cellX = startOffset.x + (cellWidth + horizontalSpacing) * positionX;
                  SetChildAlongAxis(rectChildren[i], 0, cellX, cellWidth);
                } else {
                  int positionY = i / columnCount;
                  float cellY = startOffset.y + (cellHeight + verticalSpacing) * positionY;
                  SetChildAlongAxis(rectChildren[i], 1, cellY, cellHeight);
                }
            }
        }
    }
}