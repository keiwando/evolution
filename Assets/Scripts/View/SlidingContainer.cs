using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Keiwando.UI {
    
    [RequireComponent(typeof(RectTransform))]
    public class SlidingContainer: MonoBehaviour {

        public enum Direction {
            Left, Right, Up, Down
        }

        public float SlideMultiplier {
            get => slideMultiplier;
            set => slideMultiplier = value;
        }
        [SerializeField] private float slideMultiplier = 1f;
    
        public Direction LastSlideDirection { get; private set; }

        private RectTransform rectTransform;
        
        /// The animation target positions. Needed so that the animation can be finished 
        /// instantly when a running coroutine is stopped.
        private Vector2 targetMin;
        private Vector2 targetMax;
        private Coroutine coroutine;
        public float AnimationProgress { get; private set; } = 1f;

        void Start() {
            this.rectTransform = GetComponent<RectTransform>();
        }

        public void Slide(Direction direction, float duration = 0f, float startProgress = 0f) {

            FinishAnimation();
            this.coroutine = StartCoroutine(SlideAnimation(direction, duration, startProgress));
        }

        private IEnumerator SlideAnimation(Direction direction, float duration, float startProgress) {

            var elapsed = startProgress * duration;

            LastSlideDirection = direction;

            var bottomLeft = rectTransform.offsetMin;
            var topRight = rectTransform.offsetMax;
            var relativeHeight = topRight.y - bottomLeft.y;
            var relativeWidth = topRight.x - bottomLeft.x;

            float dX = 0f;
            float dY = 0f;

            switch (direction) {
            case Direction.Left:
                dX = -relativeWidth; break;
            case Direction.Right:
                dX = relativeWidth; break;
            case Direction.Up: 
                dY = relativeHeight; break;
            case Direction.Down:
                dY = -relativeHeight; break;
            }

            bottomLeft.x += dX * slideMultiplier;
            topRight.x += dX * slideMultiplier;
            bottomLeft.y += dY * slideMultiplier;
            topRight.y += dY * slideMultiplier;

            this.targetMin = bottomLeft;
            this.targetMax = topRight;

            var oldBottomLeft = rectTransform.offsetMin;
            var oldTopRight = rectTransform.offsetMax;

            while (elapsed < duration) {

                var t = elapsed / duration;
                rectTransform.offsetMin = Vector2.Lerp(oldBottomLeft, bottomLeft, t);
                rectTransform.offsetMax = Vector2.Lerp(oldTopRight, topRight, t);
                elapsed += Time.deltaTime;
                this.AnimationProgress = t;
                yield return new WaitForEndOfFrame();
            }

            rectTransform.offsetMin = bottomLeft;
            rectTransform.offsetMax = topRight;

            this.AnimationProgress = 1f;
        }

        private void FinishAnimation() {
            if (coroutine != null) {
                StopCoroutine(coroutine);
                rectTransform.offsetMin = targetMin;
                rectTransform.offsetMax = targetMax;
                this.AnimationProgress = 1f;
            }
        }
    }
}