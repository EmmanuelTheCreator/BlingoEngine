using System;
using System.Threading.Tasks;
using AbstUI.Blazor;
using AbstUI.Blazor.Inputs;
using AbstUI.Inputs;
using LingoEngine.Bitmaps;
using LingoEngine.Events;
using LingoEngine.Inputs;
using Microsoft.JSInterop;

namespace LingoEngine.Blazor.Inputs;

/// <summary>
/// Blazor-specific mouse implementation that bridges LingoEngine with the
/// AbstUI.Blazor <see cref="BlazorMouse{T}"/> backend.
/// </summary>
public class LingoBlazorMouse : BlazorMouse<LingoMouseEvent>, ILingoFrameworkMouse
{
    private readonly AbstUIScriptResolver _scripts;

    public LingoBlazorMouse(Lazy<AbstMouse<LingoMouseEvent>> mouse, IJSRuntime js, AbstUIScriptResolver scripts)
        : base(mouse, js)
    {
        _scripts = scripts;
    }

    /// <summary>
    /// Sets a custom cursor image using a Blazor data URL.
    /// </summary>
    public void SetCursor(LingoMemberBitmap? image)
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

