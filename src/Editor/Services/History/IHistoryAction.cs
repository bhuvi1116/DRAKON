namespace DrakonNx.Editor.Services.History;

public interface IHistoryAction
{
    string Description { get; }
    void Undo();
    void Redo();
}
