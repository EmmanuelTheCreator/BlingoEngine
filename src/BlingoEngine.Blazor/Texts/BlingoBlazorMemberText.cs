using AbstUI.Blazor;
using AbstUI.Styles;
using BlingoEngine.Texts;
using BlingoEngine.Texts.FrameworkCommunication;
using Microsoft.JSInterop;

namespace BlingoEngine.Blazor.Texts;

/// <summary>
/// Concrete Blazor text member implementation.
/// </summary>
public class BlingoBlazorMemberText : BlingoBlazorMemberTextBase<BlingoMemberText>, IBlingoFrameworkMemberText
{
    public BlingoBlazorMemberText(IJSRuntime js, AbstUIScriptResolver scripts, IAbstFontManager fontManager)
        : base(js, scripts, fontManager) { }
}


