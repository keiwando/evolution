using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class HistoryManager: MonoBehaviour {

    /// <summary>
    /// The stack of previous states that can be restored with a call to <see cref="HistoryManager.Undo()"/>.
    /// </summary>
    private Stack<EditorState> undoStack = new Stack<EditorState>();
    
    /// <summary>
    /// The stack of future states that can be restored with a call to <see cref="HistoryManager.Redo()"/>.
    /// </summary>
    private Stack<EditorState> redoStack = new Stack<EditorState>();

    [SerializeField]
    private CreatureEditor editor;

    public void Push(EditorState state) {

        undoStack.Push(state);
        redoStack.Clear();
    }

    public bool CanUndo() {
        return undoStack.Count > 0;
    }

    public bool CanRedo() {
        return redoStack.Count > 0;
    }

    public void Undo() {

        if (undoStack.Count == 0) return;
        var state = undoStack.Pop();
        redoStack.Push(editor.GetState());
        editor.Refresh(state);
    }

    public void Redo() {

        if (redoStack.Count == 0) return;
        var state = redoStack.Pop();
        undoStack.Push(editor.GetState());
        editor.Refresh(state);
    }

    public string GetDebugState() {
        var stringBuilder = new StringBuilder();
        var tempStack = new Stack<EditorState>();

        stringBuilder.AppendLine("Undo Stack:");
        while (undoStack.Count > 0) {
            var state = undoStack.Pop();
            stringBuilder.AppendLine(state.CreatureDesign.GetDebugDescription());
            tempStack.Push(state);
        }
        while (tempStack.Count > 0) 
            undoStack.Push(tempStack.Pop());

        stringBuilder.AppendLine("Redo Stack:");
        while (redoStack.Count > 0) {
            var state = redoStack.Pop();
            stringBuilder.AppendLine(state.CreatureDesign.GetDebugDescription());
            tempStack.Push(state);
        }
        while (tempStack.Count > 0) 
            redoStack.Push(tempStack.Pop());

        return stringBuilder.ToString();
    }
}