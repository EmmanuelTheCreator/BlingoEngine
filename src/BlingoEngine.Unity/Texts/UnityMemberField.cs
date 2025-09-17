using AbstUI.Styles;
using BlingoEngine.Texts;
using BlingoEngine.Texts.FrameworkCommunication;
using Microsoft.Extensions.Logging;

namespace BlingoEngine.Unity.Texts;

public class UnityMemberField : UnityMemberTextBase<BlingoMemberField>, IBlingoFrameworkMemberField
{
    public UnityMemberField(IAbstFontManager fontManager, ILogger<UnityMemberField> logger) : base(fontManager, logger) { }
}

