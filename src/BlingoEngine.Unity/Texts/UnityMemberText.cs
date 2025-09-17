using AbstUI.Styles;
using BlingoEngine.Texts;
using BlingoEngine.Texts.FrameworkCommunication;
using Microsoft.Extensions.Logging;

namespace BlingoEngine.Unity.Texts;

public class UnityMemberText : UnityMemberTextBase<BlingoMemberText>, IBlingoFrameworkMemberText
{
    public UnityMemberText(IAbstFontManager fontManager, ILogger<UnityMemberText> logger) : base(fontManager, logger) { }
}

