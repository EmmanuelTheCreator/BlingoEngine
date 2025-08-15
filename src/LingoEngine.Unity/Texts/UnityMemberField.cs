using AbstUI.Styles;
using LingoEngine.Texts;
using LingoEngine.Texts.FrameworkCommunication;

namespace LingoEngine.Unity.Texts;

public class UnityMemberField : UnityMemberTextBase<LingoMemberField>, ILingoFrameworkMemberField
{
    public UnityMemberField(IAbstFontManager fontManager) : base(fontManager) { }
}
