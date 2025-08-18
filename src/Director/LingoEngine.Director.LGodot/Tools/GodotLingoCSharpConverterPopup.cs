using Godot;
using LingoEngine.Director.Core.Windowing;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Director.Core.Styles;
using LingoEngine.FrameworkCommunication;
using AbstUI.LGodot.Primitives;

namespace LingoEngine.Director.LGodot.Tools;

public partial class GodotLingoCSharpConverterPopup : Window, IDirFrameworkDialog, IFrameworkFor<LingoCSharpConverterPopup>
{
    public void Init()
    {
        var lingoEdit = FindChild("LingoText", recursive: true, owned: false) as TextEdit;
        if (lingoEdit == null)
            return;

        var highlighter = new CodeHighlighter();

        var keywordColor = DirectorColors.CodeKeyword.ToGodotColor();
        foreach (var word in new[]
        {
            "property","on","if","then","else","me","or","and","true","false","repeat","with","end","to","return","while","the","new"
        })
            highlighter.AddKeywordColor(word, keywordColor);

        var builtInColor = DirectorColors.CodeBuiltIn.ToGodotColor();
        foreach (var word in new[]
        {
            "_movie","actorlist","deleteOne","member","preload","append","membernum","sprite","spritenum","getpos","deleteone","addprop","sendsprite","voidp","frame","go","exit","line","value","cursor","sound","puppet","script","count","getpos","handler","getNetText","stepFrame","beginsprite","endsprite","startmovie","stopmovie","mouseup","mousedown","mouseenter","mouseleave","neterror","nettextresult","locH","locV","locZ","blend","ink","mouseH","mouseV","_key","keypressed","controldown","shiftdown","point","loc","in","alert","void","char","length","text","string"
        })
            highlighter.AddKeywordColor(word, builtInColor);

        var literalColor = DirectorColors.CodeLiteral.ToGodotColor();
        highlighter.NumberColor = literalColor;

        highlighter.AddColorRegion("--", "\n", DirectorColors.CodeComment.ToGodotColor());
        highlighter.AddColorRegion("\"", "\"", literalColor);
        highlighter.AddColorRegion("#", " ", builtInColor);
        highlighter.AddColorRegion("#", "\n", builtInColor);

        lingoEdit.SyntaxHighlighter = highlighter;
    }
}
