using AbstUI.Blazor;
using AbstUI.Styles;
using BlingoEngine.Texts;
using BlingoEngine.Texts.FrameworkCommunication;
using Microsoft.JSInterop;

namespace BlingoEngine.Blazor.Texts;

/// <summary>
/// Blazor implementation for field members, reusing the text base logic.
/// </summary>
public class BlingoBlazorMemberField : BlingoBlazorMemberTextBase<BlingoMemberField>, IBlingoFrameworkMemberField
{
    public BlingoBlazorMemberField(IJSRuntime js, AbstUIScriptResolver scripts, IAbstFontManager fontManager)
        : base(js, scripts, fontManager) { }
}


