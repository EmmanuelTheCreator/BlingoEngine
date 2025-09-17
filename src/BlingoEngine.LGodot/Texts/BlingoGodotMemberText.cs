using Godot;
using AbstUI.Styles;
using BlingoEngine.Texts;
using BlingoEngine.Texts.FrameworkCommunication;
using Microsoft.Extensions.Logging;

namespace BlingoEngine.LGodot.Texts
{
    public class BlingoGodotMemberText : BlingoGodotMemberTextBase<BlingoMemberText>, IBlingoFrameworkMemberText
    {
        public BlingoGodotMemberText(IAbstFontManager blingoFontManager, ILogger<BlingoGodotMemberText> logger) : base(blingoFontManager, logger)
        {
        }
        internal Node CreateForSpriteDraw() => CreateForSpriteDraw(new BlingoGodotMemberText(_fontManager, (ILogger<BlingoGodotMemberText>)_logger));
    }
}

