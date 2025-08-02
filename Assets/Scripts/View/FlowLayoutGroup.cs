using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Layout/FlowLayoutGroup")]
public class FlowLayoutGroup : LayoutGroup {
    public float spacingX = 0f;
    public float spacingY = 0f;

    public override void CalculateLayoutInputHorizontal() {
        base.CalculateLayoutInputHorizontal();
        SetLayoutAlongAxis(0);
    }

    public override void CalculateLayoutInputVertical() {
        SetLayoutAlongAxis(1);
    }

    public override void SetLayoutHorizontal() {
        SetLayoutAlongAxis(0);
    }

    public override void SetLayoutVertical() {
        SetLayoutAlongAxis(1);
    }

    private void SetLayoutAlongAxis(int axis) {
        
        if (axis == 0) {
            return;
        }
        
        float layoutWidth = rectTransform.rect.width;
        float x = padding.left;
        float y = padding.top;
        float lineHeight = 0f;
        int currentRowItemCount = 0;
        int childIndex = 0;
        
        while (childIndex < rectChildren.Count) {
          RectTransform child = rectChildren[childIndex];

            float childWidth = LayoutUtility.GetPreferredSize(child, 0);
            float childHeight = LayoutUtility.GetPreferredSize(child, 1);

            bool childDoesNotFitInRow = (x + childWidth) > (layoutWidth - padding.right);
            if (childDoesNotFitInRow && currentRowItemCount != 0) {
                x = padding.left;
                y += lineHeight + spacingY;
                lineHeight = 0f;
                currentRowItemCount = 0;
                continue;
            }

            SetChildAlongAxis(child, 0, x, childWidth);
            SetChildAlongAxis(child, 1, y, childHeight);

            x += childWidth + spacingX;
            lineHeight = Mathf.Max(lineHeight, childHeight);

            currentRowItemCount += 1;

            childIndex += 1;
        }

        float totalHeight = y + lineHeight + padding.bottom;
        SetLayoutInputForAxis(layoutWidth, layoutWidth, -1, 0);
        SetLayoutInputForAxis(totalHeight, totalHeight, -1, 1);
    }
}