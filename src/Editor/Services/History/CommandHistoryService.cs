namespace DrakonNx.Editor.Services.History;

public sealed class CommandHistoryService
{
    private readonly Stack<IHistoryAction> _undo = new();
    private readonly Stack<IHistoryAction> _redo = new();

    public bool CanUndo => _undo.Count > 0;
    public bool CanRedo => _redo.Count > 0;
    public string CurrentUndoDescription => _undo.Count > 0 ? _undo.Peek().Description : "—";
    public string CurrentRedoDescription => _redo.Count > 0 ? _redo.Peek().Description : "—";

    public void Reset()
    {
        _undo.Clear();
        _redo.Clear();
    }

    public void Execute(IHistoryAction action)
    {
        ArgumentNullException.ThrowIfNull(action);
        action.Redo();
        _undo.Push(action);
        _redo.Clear();
    }

    public string Undo()
    {
        if (!CanUndo)
        {
            throw new InvalidOperationException("Нет доступных изменений для Undo.");
        }

        var action = _undo.Pop();
        action.Undo();
        _redo.Push(action);
        return action.Description;
    }

    public string Redo()
    {
        if (!CanRedo)
        {
            throw new InvalidOperationException("Нет доступных изменений для Redo.");
        }

        var action = _redo.Pop();
        action.Redo();
        _undo.Push(action);
        return action.Description;
    }
}
