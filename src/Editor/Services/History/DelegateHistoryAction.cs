namespace DrakonNx.Editor.Services.History;

public sealed class DelegateHistoryAction : IHistoryAction
{
    private readonly Action _undo;
    private readonly Action _redo;

    public DelegateHistoryAction(string description, Action undo, Action redo)
    {
        Description = string.IsNullOrWhiteSpace(description) ? "Историческое действие" : description;
        _undo = undo ?? throw new ArgumentNullException(nameof(undo));
        _redo = redo ?? throw new ArgumentNullException(nameof(redo));
    }

    public string Description { get; }

    public void Undo() => _undo();

    public void Redo() => _redo();
}
