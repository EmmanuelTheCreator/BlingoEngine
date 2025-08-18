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
    private CodeHighlighter _csharpHighlighter;
    
    public GodotLingoCSharpConverterPopup()
    {
        _lingoHighlighter = CreateLingoHighlighter();
        _csharpHighlighter = CreateCSharpHighlighter();
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
        ((TextEdit)inputText.Framework<AbstGodotInputText>().FrameworkNode).SyntaxHighlighter = _csharpHighlighter;
    }

    public void UpdateLingoColors(AbstInputText inputText)
    {
        ((TextEdit)inputText.Framework<AbstGodotInputText>().FrameworkNode).SyntaxHighlighter = _lingoHighlighter;
    }

    private static CodeHighlighter CreateCSharpHighlighter()
    {
        var highlighter = new CodeHighlighter();

        var keywordColor = new Color("#0000FF"); // Blue
        foreach (var word in new[]
        {
            "abstract","as","base","break","case","catch","checked","class","const","continue",
            "default","delegate","do","else","enum","event","explicit","extern","finally","fixed",
            "for","foreach","goto","if","implicit","in","interface","internal","is","lock",
            "namespace","new","operator","out","override","params","private","protected","public",
            "readonly","record","ref","return","sealed","sizeof","stackalloc","static","struct",
            "switch","this","throw","try","typeof","unsafe","using","virtual","volatile","while",
            "async","await","var","null","true","false"
        })
            highlighter.AddKeywordColor(word, keywordColor);

        var typeColor = new Color("#2B91AF"); // Teal
        foreach (var word in new[]
        {
            "bool","byte","sbyte","char","decimal","double","float","int","uint","nint","nuint",
            "long","ulong","object","short","ushort","string","void"
        })
            highlighter.AddKeywordColor(word, typeColor);

        var stringColor = new Color("#A31515"); // Maroon
        var commentColor = new Color("#008000"); // Green
        var numberColor = new Color("#098658"); // Dark green

        highlighter.NumberColor = numberColor;

        highlighter.AddColorRegion("//", "", commentColor, true);
        highlighter.AddColorRegion("/*", "*/", commentColor);
        highlighter.AddColorRegion("\"", "\"", stringColor);
        highlighter.AddColorRegion("@\"", "\"", stringColor);

        return highlighter;
    }
}
