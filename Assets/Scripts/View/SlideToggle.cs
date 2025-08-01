using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class SlideToggle : MonoBehaviour {

  private const float ANIMATION_DURATION = 0.2f;

  [SerializeField] private Image background;
  [SerializeField] private Image handle;
  [SerializeField] private Color offTint = Color.grey;

  public Toggle toggle { 
    get {
      if (_toggle == null) {
        _toggle = GetComponent<Toggle>();
      }
      return _toggle;
    }
  }
  private Toggle _toggle;
  public bool animateChanges = true;

  private Color onTint;
  private float toggleEnabledHandleX;
  private Coroutine currentAnimation;
  private bool animationsEnabled = true;
  private bool initialized = false;

  void Start() {
    InitializeIfNecessary();
  }

  void InitializeIfNecessary() {
    if (initialized) {
      return;
    }
    initialized = true;
    toggle.onValueChanged.AddListener(OnValueChanged);
    toggleEnabledHandleX = handle.rectTransform.anchoredPosition.x;
    onTint = background.color;
    SetIsOn(toggle.isOn, false);
  }

  public void SetIsOn(bool isOn, bool animated = true) {
    InitializeIfNecessary();
    animationsEnabled = animated;
    bool toggleValueChanged = toggle.isOn != isOn;
    toggle.isOn = isOn;
    if (!toggleValueChanged) {
      // We run this manually to ensure that the visual state of the slide
      // toggle is updated, even if the underlying toggle might already
      // be on this state.
      OnValueChanged(isOn); 
    }
    animationsEnabled = true;
  }

  void OnValueChanged(bool isOn) {
    if (currentAnimation != null) {
      StopCoroutine(currentAnimation);
    }
    if (animateChanges && animationsEnabled && Time.deltaTime != 0) {
      currentAnimation = StartCoroutine(AnimateToggle(isOn));
    } else {
      SetHandlePosition(isOn, handle.rectTransform.anchoredPosition.x, 1f);
      SetTintColor(isOn);
    }
  }

  private IEnumerator AnimateToggle(bool isOn) {
    float time = 0f;
    float t = 0f;

    float startX = handle.rectTransform.anchoredPosition.x;

    while (time < ANIMATION_DURATION) {
      time += Time.deltaTime;
      t = time / ANIMATION_DURATION;
      t = Easing.EaseInOutCubic(t);
      SetHandlePosition(isOn, startX, t);
      if (t >= 0.5) {
        SetTintColor(isOn);
      }
      yield return null;
    }
    SetHandlePosition(isOn, startX, 1f);
    SetTintColor(isOn);
  }

  private void SetHandlePosition(bool isOn, float startX, float t) {
    float targetX = isOn ? toggleEnabledHandleX : -toggleEnabledHandleX;
    float x = Mathf.Lerp(startX, targetX, t);
    Vector2 position = handle.rectTransform.anchoredPosition;
    position.x = x;
    handle.rectTransform.anchoredPosition = position;
  }

  private void SetTintColor(bool isOn) {
    background.color = isOn ? onTint : offTint;
  }
}
