using AbstUI.Blazor;
using AbstUI.Styles;
using LingoEngine.Texts;
using LingoEngine.Texts.FrameworkCommunication;
using Microsoft.JSInterop;

namespace LingoEngine.Blazor.Texts;

/// <summary>
/// Concrete Blazor text member implementation.
/// </summary>
public class LingoBlazorMemberText : LingoBlazorMemberTextBase<LingoMemberText>, ILingoFrameworkMemberText
{
    public LingoBlazorMemberText(IJSRuntime js, AbstUIScriptResolver scripts, IAbstFontManager fontManager)
        : base(js, scripts, fontManager) { }
}

