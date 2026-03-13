using DrakonNx.Core.Model;

namespace DrakonNx.Editor.Services;

public sealed record DocumentHistoryState(
    DiagramDocument Document,
    string CurrentFilePath,
    string OpenFilePath,
    string SaveAsFilePath,
    bool IsDirty,
    string Description);
