using Godot;
using AbstUI.Styles;
using BlingoEngine.Texts;
using BlingoEngine.Texts.FrameworkCommunication;
using Microsoft.Extensions.Logging;

namespace BlingoEngine.LGodot.Texts
{
    public class BlingoGodotMemberField : BlingoGodotMemberTextBase<BlingoMemberField>, IBlingoFrameworkMemberField
    {


        public BlingoGodotMemberField(IAbstFontManager blingoFontManager, ILogger<BlingoGodotMemberField> logger) : base(blingoFontManager, logger)
        {
        }
        internal Node CreateForSpriteDraw() => CreateForSpriteDraw(new BlingoGodotMemberField(_fontManager, (ILogger<BlingoGodotMemberField>)_logger));


    }
}

