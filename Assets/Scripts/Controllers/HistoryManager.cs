using System.Collections.Generic;

public class HistoryManager {

    /// <summary>
    /// The stack of previous states that can be restored with a call to <see cref="HistoryManager.Undo()"/>.
    /// </summary>
    private Stack<EditorState> undoStack = new Stack<EditorState>();
    
    /// <summary>
    /// The stack of future states that can be restored with a call to <see cref="HistoryManager.Redo()"/>.
    /// </summary>
    private Stack<EditorState> redoStack = new Stack<EditorState>();

    private CreatureEditor editor;

    public HistoryManager(CreatureEditor editor) {
        this.editor = editor;
    }

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
        editor.Refresh(state);
    }

    public void Redo() {

        if (redoStack.Count == 0) return;
        var state = redoStack.Pop();
        editor.Refresh(state);
    }
}