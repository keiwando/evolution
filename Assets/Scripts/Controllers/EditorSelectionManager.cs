using System;
using System.Collections.Generic;
using UnityEngine;

namespace Keiwando.Evolution {

    public class EditorSelectionManager {

        private static RaycastHit[] _cachedPointCollisions = new RaycastHit[10];
        private static int _cachedPointCollisionsLength = 0;

        /// <summary>
        /// The treshold distance used to distinguish between a click/touch and a drag
        /// </summary>
        private const float TAP_THRESHOLD = 0.005f;

        private CreatureEditor editor;

        private Transform selectionArea;

        private Vector3 areaStart;
        private Vector3 currentAreaEnd;

        private List<BodyComponent> selection = new List<BodyComponent>();

        private BodyComponent currentHovering;
        private BodyComponent lastHovering;

        private Texture2D mouseDeleteTexture;

        public EditorSelectionManager(CreatureEditor editor,
                                      Transform selectionArea,
                                      Texture2D mouseDeleteTexture) {
            this.editor = editor;
            this.selectionArea = selectionArea;
            this.mouseDeleteTexture = mouseDeleteTexture;
            selectionArea.gameObject.SetActive(false);
        }

        public void Update(Vector3 mouseScreenPos) {

            UpdateHoveringSelection(mouseScreenPos);
            UpdateSelectionHighlights();
            UpdateCursor();
        }

        private void UpdateHoveringSelection(Vector3 mouseScreenPos) {

            // Figure out which body components the mouse is hovering over
            BodyComponent hovering = null;

            switch (editor.SelectedTool) {

            case CreatureEditor.Tool.Bone: 
                hovering = GetComponentAtScreenPoint<Joint>(mouseScreenPos); 
                break;

            case CreatureEditor.Tool.Muscle:
                hovering = GetComponentAtScreenPoint<Bone>(mouseScreenPos); 
                break;

            case CreatureEditor.Tool.Move:
                hovering = GetComponentAtScreenPoint<Joint>(mouseScreenPos);
                if (hovering == null)
                    hovering = CheckCachedCollisionsFor<Bone>();
                break;

            case CreatureEditor.Tool.Delete:
                hovering = GetComponentAtScreenPoint<Joint>(mouseScreenPos);
                if (hovering == null)
                    hovering = CheckCachedCollisionsFor<Bone>();
                if (hovering == null)
                    hovering = CheckCachedCollisionsFor<Muscle>();
                break;

            default: break;
            }

            lastHovering = currentHovering;
            currentHovering = hovering;
        }

        private void UpdateSelectionHighlights() {
            
            if (lastHovering != null && !selection.Contains(lastHovering)) {
                lastHovering.DisableHighlight();
            }
            if (selection.Count == 0) {
                currentHovering?.EnableHighlight();
            }
        }

        private void UpdateCursor() {
            var hotspot = Vector2.zero;
            Texture2D texture = null;
            if (editor.SelectedTool == CreatureEditor.Tool.Delete 
                && currentHovering != null) {
                texture = this.mouseDeleteTexture;
                hotspot = new Vector2(texture.width / 2, texture.height / 2);
            }
            Cursor.SetCursor(texture, hotspot, CursorMode.Auto);
        }

        public T GetSingleSelected<T>() where T: BodyComponent {
            
            if (currentHovering == null) { return null; }
            if (currentHovering is T) { return currentHovering as T; }
            return null;
        }

        public HashSet<Joint> GetJointsToMoveFromSelection() {
            
            HashSet<Joint> jointsToMove = new HashSet<Joint>();
            for (int i = 0; i < selection.Count; i++) {
                var component = selection[i];
                if (component is Joint) {
                    jointsToMove.Add(component as Joint);
                } else if (component is Bone) {
                    var bone = component as Bone;
                    jointsToMove.Add(bone.startingJoint);
                    jointsToMove.Add(bone.endingJoint);
                }
            }
            return jointsToMove;
        }

        public void AddCurrentHoveringToSelection() {
            if (currentHovering != null)
                selection.Add(currentHovering);
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
                SelectInArea<BodyComponent>(GetSelectionRect(), this.selection);
            }
        }

        public void EndSelection() {
            
            selectionArea.gameObject.SetActive(false);

            if (selectionArea.localScale == Vector3.zero) return;

            // Check if this was actually a touch/click instead of a drag
            // and adjust the selection algorithm accordingly if necessary
            if (Vector3.Distance(areaStart, currentAreaEnd) < TAP_THRESHOLD) {
                var selectionRect = new Rect(areaStart.x, areaStart.y, TAP_THRESHOLD, TAP_THRESHOLD);
                bool jointSelected = SelectInArea<Joint>(selectionRect, this.selection);
                if (!jointSelected) {
                    bool boneSelected = SelectInArea<Bone>(selectionRect, this.selection);
                    if (!boneSelected) {
                        SelectInArea<Muscle>(selectionRect, this.selection);
                    }
                }
            } else {
                SelectInArea<BodyComponent>(GetSelectionRect(), this.selection);
            }

            selectionArea.localScale = Vector3.zero;
        } 

        public void DeselectAll() {

            foreach (var item in selection) {
                item.DisableHighlight();
            }
            selection.Clear();
        }

        public bool IsAnythingSelected() {
            return this.selection.Count > 0;
        }

        public List<BodyComponent> GetSelection() {
            return this.selection;
        }

        public List<T> GetSelectedParts<T>() 
            where T: BodyComponent {
                
            var result = new List<T>();
            for (int i = 0; i < selection.Count; i++)
                if (selection[i] is T)
                    result.Add(selection[i] as T);
            return result;
        }

        public bool DeleteSelection() {

            bool creatureChanged = this.selection.Count > 0;
            editor.creatureBuilder.Delete(this.selection);
            BodyComponent.RemoveDeletedObjects(this.selection);
            return creatureChanged;
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

        private bool SelectInArea<T>(Rect selectionArea, List<BodyComponent> selection)
            where T: BodyComponent {

            GetComponentsInRect<T>(selectionArea, selection);
            foreach (var item in selection) {
                item.EnableHighlight();
            }

            DisableHighlightsOnNonSelected();
            
            return this.selection.Count > 0;
        }

        private void DisableHighlightsOnNonSelected() {
            // TODO: Make this more efficient
            var all = GameObject.FindObjectsOfType<BodyComponent>();
            foreach (var bodyComponent in all) {
                if (!this.selection.Contains(bodyComponent)) {
                    bodyComponent.DisableHighlight();
                }
            }
        }

        private static void GetComponentsInRect<T>(Rect rect, List<BodyComponent> result) 
            where T: BodyComponent {

            Collider[] colliders = Physics.OverlapBox(rect.center, rect.size * 0.5f);
            
            result.Clear();

            for (int i = 0; i < colliders.Length; i++) {
                var bodyComponent = colliders[i].GetComponent<T>();
                if (bodyComponent != null) {
                    result.Add(bodyComponent);
                }
            }
        }

        private static T GetComponentAtScreenPoint<T>(Vector3 point) 
            where T: BodyComponent {

            Ray ray = Camera.main.ScreenPointToRay(point);
            var hitCount = Physics.RaycastNonAlloc(ray, _cachedPointCollisions);
            _cachedPointCollisionsLength = Math.Min(hitCount, 10); 

            return CheckCachedCollisionsFor<T>();
        }

        private static T CheckCachedCollisionsFor<T>() where T: BodyComponent {
            for (int i = 0; i < _cachedPointCollisionsLength; i++) {
                if (_cachedPointCollisions[i].collider == null) return null;
                T component = _cachedPointCollisions[i].collider.GetComponent<T>();
                if (component == null) continue;
                return component;
            }
            return null;
        }
    }
}