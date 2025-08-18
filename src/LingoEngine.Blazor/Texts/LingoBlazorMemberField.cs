using AbstUI.Blazor;
using LingoEngine.Texts;
using LingoEngine.Texts.FrameworkCommunication;
using Microsoft.JSInterop;

namespace LingoEngine.Blazor.Texts;

/// <summary>
/// Blazor implementation for field members, reusing the text base logic.
/// </summary>
public class LingoBlazorMemberField : LingoBlazorMemberTextBase<LingoMemberField>, ILingoFrameworkMemberField
{
    public LingoBlazorMemberField(IJSRuntime js, AbstUIScriptResolver scripts) : base(js, scripts) { }
}

