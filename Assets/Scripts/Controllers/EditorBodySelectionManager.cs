using System;
using UnityEngine;

namespace Keiwando.Evolution {

    public class EditorBodySelectionManager {

        private CreatureEditor editor;
        private CreatureBuilder builder;

        private Transform selectionArea;

        private Vector3 areaStart;
        private Vector3 currentAreaEnd;

        public EditorBodySelectionManager(CreatureEditor editor, 
                                          CreatureBuilder creatureBuilder,
                                          Transform selectionArea) {
            this.builder = creatureBuilder;
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
        }

        public void EndSelection() {
            // TODO: Implement
            selectionArea.gameObject.SetActive(false);
        } 

        public void DeselectAll() {
            // TODO: Implement
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
    }
}