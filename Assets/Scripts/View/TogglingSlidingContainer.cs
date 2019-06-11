using UnityEngine;

namespace Keiwando.UI {

    public class TogglingSlidingContainer: SlidingContainer {

        [SerializeField] private Direction initialDirection;

        protected override void Start() {
            base.Start();

            LastSlideDirection = Opposite(initialDirection);
            AnimationProgress = 1f;
        }

        public void Slide(float duration = 0f) {

            base.Slide(Opposite(LastSlideDirection), duration, 1f - AnimationProgress);
        }

        private Direction Opposite(Direction dir) {
            switch (dir) {
                case Direction.Down: return Direction.Up;
                case Direction.Up: return Direction.Down;
                case Direction.Left: return Direction.Right;
                case Direction.Right: return Direction.Left;
                default: throw new System.ArgumentException("Opposite direction not known");
            }
        }
    }
}