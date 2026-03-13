using Avalonia.Threading;

namespace DrakonNx.Editor.Services;

public static class UiDispatcher
{
    public static Task InvokeAsync(Action action)
    {
        return Dispatcher.UIThread.InvokeAsync(action).GetTask();
    }
}
