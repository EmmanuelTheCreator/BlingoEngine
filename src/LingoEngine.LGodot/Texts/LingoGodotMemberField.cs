using Godot;
using LingoEngine.Styles;
using LingoEngine.Texts;
using LingoEngine.Texts.FrameworkCommunication;
using Microsoft.Extensions.Logging;

namespace LingoEngine.LGodot.Texts
{
    public class LingoGodotMemberField : LingoGodotMemberTextBase<LingoMemberField>, ILingoFrameworkMemberField
    {


        public LingoGodotMemberField(ILingoFontManager lingoFontManager, ILogger<LingoGodotMemberField> logger) : base(lingoFontManager, logger)
        {
        }
        internal Node CreateForSpriteDraw() => CreateForSpriteDraw(new LingoGodotMemberField(_fontManager, (ILogger<LingoGodotMemberField>)_logger));


    }
}
