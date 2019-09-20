using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class UIFade : MonoBehaviour {

	private const float DEFAULT_FADE = 0.2f;

	private CanvasGroup alphaTarget;
	private Coroutine routine;
	
	void Start () {
		RefreshTarget();
	}
	
	public void FadeInOut(float totalDuration = 1.0f, float fadeDuration = DEFAULT_FADE) {
		
		gameObject.SetActive(true);
		RefreshTarget();
		ResetRoutine();
		routine = StartCoroutine(FadeInOutAnim(totalDuration, fadeDuration));
	}

	public void FadeIn(float fadeDuration = DEFAULT_FADE) {
		gameObject.SetActive(true);
		ResetRoutine();
		routine = StartCoroutine(FadeInAnim(fadeDuration));
	}

	public void FadeOut(float fadeDuration = DEFAULT_FADE) {
		gameObject.SetActive(true);
		ResetRoutine();
		routine = StartCoroutine(FadeOutAnim(fadeDuration));
	}

	private IEnumerator FadeInAnim(float fadeDuration) {
		var elapsedTime = 0f;
		while (elapsedTime < fadeDuration) {
			alphaTarget.alpha = elapsedTime / fadeDuration;
			elapsedTime += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		alphaTarget.alpha = 1;
	}

	private IEnumerator FadeOutAnim(float fadeDuration) {
		var elapsedTime = 0f;
		while (elapsedTime < fadeDuration) {
			alphaTarget.alpha = 1f - elapsedTime / fadeDuration;
			elapsedTime += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		alphaTarget.alpha = 0f;
	}

	private IEnumerator FadeInOutAnim(float totalDuration, float fadeDuration) {

		// var elapsedTime = 0f;
		// while (elapsedTime < fadeDuration) {
		// 	alphaTarget.alpha = elapsedTime / fadeDuration;
		// 	elapsedTime += Time.deltaTime;
		// 	yield return new WaitForEndOfFrame();
		// }

		// alphaTarget.alpha = 1;
		yield return FadeInAnim(fadeDuration);
		var waitDuration = Math.Max(0f, totalDuration - 2f * fadeDuration);
		yield return new WaitForSeconds(waitDuration);
		yield return FadeOutAnim(fadeDuration);
		gameObject.SetActive(false);
		
		// elapsedTime = 0f;
		// while (elapsedTime < fadeDuration) {
		// 	alphaTarget.alpha = 1f - elapsedTime / fadeDuration;
		// 	elapsedTime += Time.deltaTime;
		// 	yield return new WaitForEndOfFrame();
		// }
		// alphaTarget.alpha = 0f;
	}

	private void RefreshTarget() {
		if (alphaTarget == null) {
			alphaTarget = GetComponent<CanvasGroup>();
		}
	}

	private void ResetRoutine(){
		if (routine != null) {
			StopCoroutine(routine);
			routine = null;
		}
	}
}
