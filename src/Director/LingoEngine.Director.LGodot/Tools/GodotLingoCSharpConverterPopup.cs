using AbstUI.Components;
using AbstUI.LGodot.Components;
using AbstUI.LGodot.Primitives;
using Godot;
using LingoEngine.Director.Core.Styles;
using LingoEngine.Director.Core.Tools;
using LingoEngine.FrameworkCommunication;

namespace LingoEngine.Director.LGodot.Tools;

public partial class GodotLingoCSharpConverterPopup : Window, ILingoFrameworkCSharpConverterPopup, IFrameworkFor<LingoCSharpConverterPopup>
{
    private CodeHighlighter _lingoHighlighter;
    
    public GodotLingoCSharpConverterPopup()
    {
        _lingoHighlighter = CreateLingoHighlighter();
    }

    
    public void Init()
    {

    }
    private static CodeHighlighter CreateLingoHighlighter()
    {
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
            "point","loc","void","char","rgb","in","line",
            "default","format","color","comment","integer","boolean","string","text","string","symbol",
            "getPropertyDescriptionList","GetBehaviorTooltip","IsOKToAttach","GetBehaviorDescription",
            "_movie","actorlist","cursor","alert",
            "membernum","member","preload","sound",
            "sprite","spritenum","locH","locV","locZ","blend","ink","mouseH","mouseV","puppet",
            "deleteOne","append","getpos","deleteone","addprop","sendsprite","voidp","frame","length","count",
            "go","exit",
            "value",
            "script","handler",
            "stepFrame","beginsprite","endsprite",
            "startmovie","stopmovie",
            "mouseup","mousedown","mouseenter","mouseleave",
            "neterror","nettextresult","getNetText",
            "_key","keypressed","controldown","shiftdown",
        })
            highlighter.AddKeywordColor(word, builtInColor);

        var literalColor = DirectorColors.CodeLiteral.ToGodotColor();
        highlighter.NumberColor = literalColor;

        highlighter.AddColorRegion("--", "", DirectorColors.CodeComment.ToGodotColor(),true);
        highlighter.AddColorRegion("\"", "\"", literalColor);
      
        return highlighter;
    }

    public void UpdateCSharpColors(AbstInputText inputText)
    {
        
    }

    public void UpdateLingoColors(AbstInputText inputText)
    {
        ((TextEdit)inputText.Framework<AbstGodotInputText>().FrameworkNode).SyntaxHighlighter = _lingoHighlighter;
    }
}
