using LingoEngine.Styles;
using LingoEngine.Texts;
using LingoEngine.Texts.FrameworkCommunication;

namespace LingoEngine.Unity.Texts;

public class UnityMemberField : UnityMemberTextBase<LingoMemberField>, ILingoFrameworkMemberField
{
    public UnityMemberField(ILingoFontManager fontManager) : base(fontManager) { }
}
