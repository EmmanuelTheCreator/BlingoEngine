using AbstUI.Styles;
using LingoEngine.Texts;
using LingoEngine.Texts.FrameworkCommunication;
using Microsoft.Extensions.Logging;

namespace LingoEngine.Unity.Texts;

public class UnityMemberText : UnityMemberTextBase<LingoMemberText>, ILingoFrameworkMemberText
{
    public UnityMemberText(IAbstFontManager fontManager, ILogger<UnityMemberText> logger) : base(fontManager, logger) { }
}
