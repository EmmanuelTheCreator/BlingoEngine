using AbstUI.Blazor;
using AbstUI.Styles;
using LingoEngine.Texts;
using LingoEngine.Texts.FrameworkCommunication;
using Microsoft.JSInterop;

namespace LingoEngine.Blazor.Texts;

/// <summary>
/// Blazor implementation for field members, reusing the text base logic.
/// </summary>
public class LingoBlazorMemberField : LingoBlazorMemberTextBase<LingoMemberField>, ILingoFrameworkMemberField
{
    public LingoBlazorMemberField(IJSRuntime js, AbstUIScriptResolver scripts, IAbstFontManager fontManager)
        : base(js, scripts, fontManager) { }
}

