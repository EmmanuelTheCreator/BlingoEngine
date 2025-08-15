using AbstUI.Styles;
using LingoEngine.Texts;
using LingoEngine.Texts.FrameworkCommunication;

namespace LingoEngine.Unity.Texts;

public class UnityMemberText : UnityMemberTextBase<LingoMemberText>, ILingoFrameworkMemberText
{
    public UnityMemberText(IAbstFontManager fontManager) : base(fontManager) { }
}
