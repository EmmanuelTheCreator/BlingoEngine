using Microsoft.JSInterop;

namespace BlingoEngine.Blazor;

public interface IBlazorWindowManager
{
    void ShowWindow(string id);
    void HideWindow(string id);
}

public class BlazorWindowManager : IBlazorWindowManager
{
    private readonly IJSRuntime _js;
    private IJSObjectReference? _module;

    public BlazorWindowManager(IJSRuntime js)
    {
        _js = js;
    }

    private void EnsureModule()
    {
        _module ??= _js.InvokeAsync<IJSObjectReference>("import", "./_content/AbstUI.Blazor/scripts/abstUIScripts.js")
                     .AsTask().GetAwaiter().GetResult();
    }

    public void ShowWindow(string id)
    {
        EnsureModule();
        _module!.InvokeVoidAsync("AbstUIWindow.showBootstrapModal", id);
    }

    public void HideWindow(string id)
    {
        EnsureModule();
        _module!.InvokeVoidAsync("AbstUIWindow.hideBootstrapModal", id);
    }
}


