using AbstUI.SDL2.Core;
using AbstUI.Styles;
using BlingoEngine.Texts;
using BlingoEngine.Texts.FrameworkCommunication;

namespace BlingoEngine.SDL2.Texts;

public class SdlMemberText : SdlMemberTextBase<BlingoMemberText>, IBlingoFrameworkMemberText
{
    public SdlMemberText(IAbstFontManager fontManager, ISdlRootComponentContext sdlRootContext) : base(fontManager, sdlRootContext) { }
}

