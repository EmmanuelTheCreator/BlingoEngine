using Godot;
using AbstUI.Styles;
using LingoEngine.Texts;
using LingoEngine.Texts.FrameworkCommunication;
using Microsoft.Extensions.Logging;

namespace LingoEngine.LGodot.Texts
{
    public class LingoGodotMemberText : LingoGodotMemberTextBase<LingoMemberText>, ILingoFrameworkMemberText
    {
        public LingoGodotMemberText(IAbstFontManager lingoFontManager, ILogger<LingoGodotMemberText> logger) : base(lingoFontManager, logger)
        {
        }
        internal Node CreateForSpriteDraw() => CreateForSpriteDraw(new LingoGodotMemberText(_fontManager, (ILogger<LingoGodotMemberText>)_logger));
    }
}
