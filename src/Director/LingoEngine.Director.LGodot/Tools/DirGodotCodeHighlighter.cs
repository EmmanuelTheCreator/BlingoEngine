using AbstUI.LGodot.Components;
using AbstUI.LGodot.Primitives;
using Godot;
using LingoEngine.Director.Core.Styles;
using LingoEngine.Director.Core.Tools;
using LingoEngine.FrameworkCommunication;
using System;

namespace LingoEngine.Director.LGodot.Tools;

public class DirGodotCodeHighlighter : IDirFrameworkCodeHighlighter, IFrameworkFor<DirCodeHighlichter>, IDisposable
{
    private DirCodeHighlichter _base = null!;
    private readonly CodeHighlighter _highlighter = new();

    public DirGodotCodeHighlighter() { }

    public void Init(DirCodeHighlichter highlighter)
    {
        _base = highlighter;
        _base.TextChanged += UpdateColors;
        Setup();
        UpdateColors();
    }

    public void Dispose()
    {
        _base.TextChanged -= UpdateColors;
    }

    private void Setup()
    {
        if (_base.CodeLanguage == DirCodeHighlichter.Language.Lingo)
        {
            var keywordColor = DirectorColors.LingoCodeKeyword.ToGodotColor();
            foreach (var word in DirCodeHighlichter.CaseInsensitiveWords(_base.WordsLingoCodeKeywords))
                _highlighter.AddKeywordColor(word, keywordColor);

            var builtInColor = DirectorColors.LingoCodeBuiltIn.ToGodotColor();
            foreach (var word in DirCodeHighlichter.CaseInsensitiveWords(_base.WordsLingoCodeBuiltIn))
                _highlighter.AddKeywordColor(word, builtInColor);

            var literalColor = DirectorColors.LingoCodeLiteral.ToGodotColor();
            _highlighter.NumberColor = literalColor;
            _highlighter.AddColorRegion("--", "", DirectorColors.LingoCodeComment.ToGodotColor(), true);
            _highlighter.AddColorRegion("\"", "\"", literalColor);
        }
        else
        {
            var keywordColor = DirectorColors.CCharpCodeBuiltIn.ToGodotColor();
            foreach (var word in _base.WordsCCharpCodeBuiltIn)
                _highlighter.AddKeywordColor(word, keywordColor);

            var typeColor = DirectorColors.CCharpCodeTypes.ToGodotColor();
            foreach (var word in _base.WordsCCharpCodeTypes)
                _highlighter.AddKeywordColor(word, typeColor);

            _highlighter.NumberColor = DirectorColors.CCharpCodeNumber.ToGodotColor();
            _highlighter.AddColorRegion("//", "", DirectorColors.CCharpCodeComment.ToGodotColor(), true);
            _highlighter.AddColorRegion("/*", "*/", DirectorColors.CCharpCodeComment.ToGodotColor());
            _highlighter.AddColorRegion("\"", "\"", DirectorColors.CCharpCodeString.ToGodotColor());
            _highlighter.AddColorRegion("@\"", "\"", DirectorColors.CCharpCodeString.ToGodotColor());
        }
    }

    private void UpdateColors()
    {
        var textEdit = (TextEdit)_base.TextComponent.Framework<AbstGodotInputText>().FrameworkNode;
        textEdit.SyntaxHighlighter = _highlighter;
    }
}
