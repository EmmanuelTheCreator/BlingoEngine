using AbstUI.SDL2.Core;
using AbstUI.Styles;
using BlingoEngine.Texts;
using BlingoEngine.Texts.FrameworkCommunication;

namespace BlingoEngine.SDL2.Texts;

public class SdlMemberField : SdlMemberTextBase<BlingoMemberField>, IBlingoFrameworkMemberField
{
    public SdlMemberField(IAbstFontManager fontManager, ISdlRootComponentContext sdlRootContext) : base(fontManager, sdlRootContext) { }
}

