using UnityEngine;
using UnityEngine.EventSystems;

public class PointerHoverDetector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

  public bool isHovered = false;

  public void OnPointerEnter(PointerEventData eventData) {
    isHovered = true;
  }

  public void OnPointerExit(PointerEventData eventData) {
    isHovered = false;
  }
}