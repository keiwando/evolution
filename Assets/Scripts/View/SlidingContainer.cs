using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Keiwando.UI {
    
    [RequireComponent(typeof(RectTransform))]
    public class SlidingContainer: MonoBehaviour {

        public enum Direction {
            Left = 0, 
            Right = 1, 
            Up = 2, 
            Down = 3
        }

        public float SlideMultiplier {
            get => slideMultiplier;
            set => slideMultiplier = value;
        }
        [SerializeField] private float slideMultiplier = 1f;

        [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
        public Direction LastSlideDirection { get; protected set; }

        /// The animation target positions. Needed so that the animation can be finished 
        /// instantly when a running coroutine is stopped.
        private Vector2 targetPosition;
        private Coroutine coroutine;
        public float AnimationProgress { get; protected set; } = 1f;

        public void Slide(Direction direction, float duration = 0f, bool ignoreProgress = true) {

            FinishAnimation();
            float startProgress = ignoreProgress ? 0f : 1f - AnimationProgress;
            this.coroutine = StartCoroutine(SlideAnimation(direction, duration,  startProgress));
        }

        private IEnumerator SlideAnimation(Direction direction, float duration, float startProgress) {
            var rectTransform = transform as RectTransform;

            var elapsed = startProgress * duration;

            LastSlideDirection = direction;

            var localWidth = rectTransform.rect.width;
            var localHeight = rectTransform.rect.height;

            float dX = 0f;
            float dY = 0f;

            switch (direction) {
            case Direction.Left:
                dX = -localWidth; break;
            case Direction.Right:
                dX = localWidth; break;
            case Direction.Up: 
                dY = localHeight; break;
            case Direction.Down:
                dY = -localHeight; break;
            }

            var startPos = rectTransform.localPosition;
            var targetPos = startPos;
            targetPos.x += dX * slideMultiplier;
            targetPos.y += dY * slideMultiplier;
            this.targetPosition = targetPos;


            while (elapsed < duration) {

                var t = animationCurve.Evaluate(elapsed / duration);
                
                rectTransform.localPosition = Vector2.Lerp(startPos, targetPos, t);
                elapsed += Time.deltaTime;
                this.AnimationProgress = t;
                yield return new WaitForEndOfFrame();
            }

            rectTransform.localPosition = targetPos;

            this.AnimationProgress = 1f;
        }

        private void FinishAnimation() {
            if (coroutine != null) {
                StopCoroutine(coroutine);
                transform.localPosition = targetPosition;
                this.AnimationProgress = 1f;
            }
        }
    }
}