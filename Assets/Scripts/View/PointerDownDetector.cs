using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PointerDownDetector : MonoBehaviour, IPointerDownHandler {

  public UnityEvent onPointerDown = new UnityEvent();

  public void OnPointerDown(PointerEventData eventData) {
    onPointerDown.Invoke();
  }
}