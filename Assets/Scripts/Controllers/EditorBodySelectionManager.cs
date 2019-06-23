using System;
using UnityEngine;

namespace Keiwando.Evolution {

    public class EditorBodySelectionManager {

        /// <summary>
        /// The treshold distance used to distinguish between a click/touch and a drag
        /// </summary>
        private const float TAP_THRESHOLD = 0.005f;

        private CreatureEditor editor;

        private Transform selectionArea;

        private Vector3 areaStart;
        private Vector3 currentAreaEnd;

        public EditorBodySelectionManager(CreatureEditor editor,
                                          Transform selectionArea) {
            this.editor = editor;
            this.selectionArea = selectionArea;
            selectionArea.gameObject.SetActive(false);
        }

        public void StartSelection(Vector3 startPosition) {
            this.areaStart = startPosition;
            this.currentAreaEnd = startPosition;
            selectionArea.gameObject.SetActive(true);
            UpdateVisualSelectionArea();
        }

        public void UpdateSelection(Vector3 currentEndPosition) {

            this.currentAreaEnd = currentEndPosition;
            UpdateVisualSelectionArea();

            // Don't prematurely highlight when it could still be counted as a 
            // touch instead of a drag (prevents flickering)
            if (Vector3.Distance(areaStart, currentAreaEnd) >= TAP_THRESHOLD) {
                editor.creatureBuilder.SelectInArea<BodyComponent>(GetSelectionRect());
            }
        }

        public void EndSelection() {
            // TODO: Implement
            selectionArea.gameObject.SetActive(false);

            // Check if this was actually a touch/click instead of a drag
            // and adjust the selection algorithm accordingly if necessary
            if (Vector3.Distance(areaStart, currentAreaEnd) < TAP_THRESHOLD) {
                var selectionRect = new Rect(areaStart.x, areaStart.y, TAP_THRESHOLD, TAP_THRESHOLD);
                bool jointSelected = editor.creatureBuilder.SelectInArea<Joint>(selectionRect);
                if (!jointSelected) {
                    bool boneSelected = editor.creatureBuilder.SelectInArea<Bone>(selectionRect);
                    if (!boneSelected) {
                        editor.creatureBuilder.SelectInArea<Muscle>(selectionRect);
                    }
                }
            } else {
                editor.creatureBuilder.SelectInArea<BodyComponent>(GetSelectionRect());
            }
        } 

        public void DeselectAll() {
            // TODO: Implement
            editor.creatureBuilder.DeselectAll();
        }

        private void UpdateVisualSelectionArea() {

            var center = new Vector3(
                0.5f * (areaStart.x + currentAreaEnd.x),
                0.5f * (areaStart.y + currentAreaEnd.y),
                0f
            );
            var scale = new Vector3(
                Math.Abs(currentAreaEnd.x - areaStart.x),
                Math.Abs(currentAreaEnd.y - areaStart.y),
                1f
            );

            selectionArea.localScale = scale;
            selectionArea.position = center;
        }

        private Rect GetSelectionRect() {
            var pos = selectionArea.position;
            var size = selectionArea.localScale;
            return new Rect(
                pos - 0.5f * size,
                size
            );
        }
    }
}