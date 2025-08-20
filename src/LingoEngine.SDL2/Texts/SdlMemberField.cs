using AbstUI.SDL2.Core;
using AbstUI.Styles;
using LingoEngine.Texts;
using LingoEngine.Texts.FrameworkCommunication;

namespace LingoEngine.SDL2.Texts;

public class SdlMemberField : SdlMemberTextBase<LingoMemberField>, ILingoFrameworkMemberField
{
    public SdlMemberField(IAbstFontManager fontManager, ISdlRootComponentContext sdlRootContext) : base(fontManager, sdlRootContext) { }
}
