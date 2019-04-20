using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public static class AnimationUtils {

    public static IEnumerator Flash(CanvasGroup canvasGroup, float showDuration, float fadeOutDuration) {

        canvasGroup.gameObject.SetActive(true);

        canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(showDuration);

        // Fade out
        float start = Time.time;
        float elapsed = 0f;

        while (elapsed < fadeOutDuration) {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = elapsed / fadeOutDuration;
            yield return new WaitForEndOfFrame();
        } 

        canvasGroup.alpha = 0f;
        canvasGroup.gameObject.SetActive(false);
    }
}