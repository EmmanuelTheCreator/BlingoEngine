using System;
using System.Threading.Tasks;
using AbstUI.Blazor;
using AbstUI.Blazor.Inputs;
using AbstUI.Inputs;
using BlingoEngine.Bitmaps;
using BlingoEngine.Events;
using BlingoEngine.Inputs;
using Microsoft.JSInterop;

namespace BlingoEngine.Blazor.Inputs;

/// <summary>
/// Blazor-specific mouse implementation that bridges BlingoEngine with the
/// AbstUI.Blazor <see cref="BlazorMouse{T}"/> backend.
/// </summary>
public class BlingoBlazorMouse : BlazorMouse<BlingoMouseEvent>, IBlingoFrameworkMouse
{
    private readonly AbstUIScriptResolver _scripts;

    public BlingoBlazorMouse(Lazy<AbstMouse<BlingoMouseEvent>> mouse, IJSRuntime js, AbstUIScriptResolver scripts)
        : base(mouse, js)
    {
        _scripts = scripts;
    }

    /// <summary>
    /// Sets a custom cursor image using a Blazor data URL.
    /// </summary>
    public void SetCursor(BlingoMemberBitmap? image)
    {
        if (image?.ImageData == null)
        {
            _ = SetCursorCss("default");
            return;
        }

        string base64 = Convert.ToBase64String(image.ImageData);
        string cursor = $"url('data:{image.Format};base64,{base64}'), auto";
        _ = SetCursorCss(cursor);
    }

  

    private Task SetCursorCss(string cursor) => _scripts.SetCursor(cursor).AsTask();
}


