using AbstUI.Components;
using AbstUI.LGodot.Components;
using AbstUI.LGodot.Primitives;
using Godot;
using LingoEngine.Director.Core.Styles;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Director.Core.Windowing;
using LingoEngine.FrameworkCommunication;

namespace LingoEngine.Director.LGodot.Tools;

public partial class GodotLingoCSharpConverterPopup : Window, ILingoFrameworkCSharpConverterPopup, IFrameworkFor<LingoCSharpConverterPopup>
{
    private CodeHighlighter _lingoHighlighter;
    private CodeHighlighter _csharpHighlighter;
    private LingoCSharpConverterPopup? _lingoDialog;
    private List<string> _wordsLingoCodeBuiltIn = new();
    private List<string> _wordsLingoCodeKeywords = new();
    private List<string> _wordsCCharpCodeBuiltIn = new();
    private List<string> _wordsCCharpCodeTypes = new();

    public GodotLingoCSharpConverterPopup()
    {
        
    }

    
    public void Init(ILingoDialog lingoDialog)
    {
        _lingoDialog = (LingoCSharpConverterPopup)lingoDialog;
        _wordsLingoCodeBuiltIn = _lingoDialog.WordsLingoCodeBuiltIn;
        _wordsLingoCodeKeywords = _lingoDialog.WordsLingoCodeKeywords;
        _wordsCCharpCodeBuiltIn = _lingoDialog.WordsCCharpCodeBuiltIn;
        _wordsCCharpCodeTypes = _lingoDialog.WordsCCharpCodeTypes;
        _lingoHighlighter = CreateLingoHighlighter();
        _csharpHighlighter = CreateCSharpHighlighter();
    }
    private CodeHighlighter CreateLingoHighlighter()
    {
        var highlighter = new CodeHighlighter();

        var keywordColor = DirectorColors.LingoCodeKeyword.ToGodotColor();
        foreach (var word in _wordsLingoCodeKeywords)
            highlighter.AddKeywordColor(word, keywordColor);

        var builtInColor = DirectorColors.LingoCodeBuiltIn.ToGodotColor();
         
        foreach (var word in _wordsLingoCodeBuiltIn)
            highlighter.AddKeywordColor(word, builtInColor);

        var literalColor = DirectorColors.LingoCodeLiteral.ToGodotColor();
        highlighter.NumberColor = literalColor;

        highlighter.AddColorRegion("--", "", DirectorColors.LingoCodeComment.ToGodotColor(),true);
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

    private CodeHighlighter CreateCSharpHighlighter()
    {
        var highlighter = new CodeHighlighter();

        var keywordColor = DirectorColors.CCharpCodeBuiltIn.ToGodotColor();
        foreach (var word in _wordsCCharpCodeBuiltIn)
            highlighter.AddKeywordColor(word, keywordColor);

        var typeColor = DirectorColors.CCharpCodeTypes.ToGodotColor();
        foreach (var word in _wordsCCharpCodeTypes)
            highlighter.AddKeywordColor(word, typeColor);

       

        highlighter.NumberColor = DirectorColors.CCharpCodeNumber.ToGodotColor(); ;

        highlighter.AddColorRegion("//", "", DirectorColors.CCharpCodeComment.ToGodotColor(), true);
        highlighter.AddColorRegion("/*", "*/", DirectorColors.CCharpCodeComment.ToGodotColor());
        highlighter.AddColorRegion("\"", "\"", DirectorColors.CCharpCodeString.ToGodotColor());
        highlighter.AddColorRegion("@\"", "\"", DirectorColors.CCharpCodeString.ToGodotColor());

        return highlighter;
    }
}
