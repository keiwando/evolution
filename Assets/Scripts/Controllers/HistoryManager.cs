using System.Collections.Generic;
using System.Text;

public class HistoryManager<State> {

    public interface IStateProvider {
        State GetState(HistoryManager<State> historyManager);
        void SetState(State state);
    }

    /// <summary>
    /// The stack of previous states that can be restored with a call to <see cref="HistoryManager.Undo()"/>.
    /// </summary>
    private Stack<State> undoStack = new Stack<State>();
    
    /// <summary>
    /// The stack of future states that can be restored with a call to <see cref="HistoryManager.Redo()"/>.
    /// </summary>
    private Stack<State> redoStack = new Stack<State>();

    private IStateProvider stateProvider; 

    public HistoryManager(IStateProvider provider) {
        this.stateProvider = provider;
    }

    public void Push(State state) {

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
        redoStack.Push(stateProvider.GetState(this));
        stateProvider.SetState(state);
    }

    public void Redo() {

        if (redoStack.Count == 0) return;
        var state = redoStack.Pop();
        undoStack.Push(stateProvider.GetState(this));
        stateProvider.SetState(state);
    }

    // public string GetDebugState() {
    //     var stringBuilder = new StringBuilder();
    //     var tempStack = new Stack<State>();

    //     stringBuilder.AppendLine("Undo Stack:");
    //     while (undoStack.Count > 0) {
    //         var state = undoStack.Pop();
    //         stringBuilder.AppendLine(state.CreatureDesign.GetDebugDescription());
    //         tempStack.Push(state);
    //     }
    //     while (tempStack.Count > 0) 
    //         undoStack.Push(tempStack.Pop());

    //     stringBuilder.AppendLine("Redo Stack:");
    //     while (redoStack.Count > 0) {
    //         var state = redoStack.Pop();
    //         stringBuilder.AppendLine(state.CreatureDesign.GetDebugDescription());
    //         tempStack.Push(state);
    //     }
    //     while (tempStack.Count > 0) 
    //         redoStack.Push(tempStack.Pop());

    //     return stringBuilder.ToString();
    // }
}