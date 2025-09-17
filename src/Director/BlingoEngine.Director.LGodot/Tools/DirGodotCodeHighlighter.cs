using AbstUI.FrameworkCommunication;
using AbstUI.LGodot.Components.Inputs;
using AbstUI.LGodot.Primitives;
using Godot;
using BlingoEngine.Director.Core.Styles;
using BlingoEngine.Director.Core.Tools;
using System;

namespace BlingoEngine.Director.LGodot.Tools;

public class DirGodotCodeHighlighter : IDirFrameworkCodeHighlighter, IFrameworkForInitializable<DirCodeHighlichter>, IDisposable
{
    private DirCodeHighlichter _base = null!;
    private readonly CodeHighlighter _highlighter = new();

    public DirGodotCodeHighlighter() { }

    public void Init(DirCodeHighlichter highlighter)
    {
        _base = highlighter;
        _base.TextChanged += UpdateColors;
        highlighter.Init(this);
    }

    public void Dispose()
    {
        _base.TextChanged -= UpdateColors;
    }
    public void Update()
    {
        Setup();
        UpdateColors();
    }
    private void Setup()
    {
        if (_base.CodeLanguage == DirCodeHighlichter.SourceCodeLanguage.Lingo)
        {
            var keywordColor = DirectorColors.BlingoCodeKeyword.ToGodotColor();
            foreach (var word in DirCodeHighlichter.CaseInsensitiveWords(_base.WordsBlingoCodeKeywords))
                _highlighter.AddKeywordColor(word, keywordColor);

            var builtInColor = DirectorColors.BlingoCodeBuiltIn.ToGodotColor();
            foreach (var word in DirCodeHighlichter.CaseInsensitiveWords(_base.WordsBlingoCodeBuiltIn))
                _highlighter.AddKeywordColor(word, builtInColor);

            var literalColor = DirectorColors.BlingoCodeLiteral.ToGodotColor();
            _highlighter.NumberColor = literalColor;
            _highlighter.AddColorRegion("--", "", DirectorColors.BlingoCodeComment.ToGodotColor(), true);
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

