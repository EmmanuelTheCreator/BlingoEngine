using AbstUI.Components;
using LingoEngine.FrameworkCommunication;

namespace LingoEngine.Director.Core.Tools;

public class DirCodeHighlichter : IDisposable
{
    public enum SourceCodeLanguage
    {
        Lingo,
        CSharp
    }

    private readonly AbstInputText _text;
    private readonly Action _onChanged;

    public DirCodeHighlichter(ILingoFrameworkFactory factory)
    {
        _text = factory.CreateInputText("CodeText", 0, null);
        _text.IsMultiLine = true;
        _onChanged = () => TextChanged?.Invoke();
        _text.ValueChanged += _onChanged;
    }

    public SourceCodeLanguage CodeLanguage { get; set; }
    public AbstInputText TextComponent => _text;

    public event Action? TextChanged;

    public float Width
    {
        get => _text.Width;
        set => _text.Width = value;
    }

    public float Height
    {
        get => _text.Height;
        set => _text.Height = value;
    }

    public void SetText(string text)
    {
        _text.Text = text;
    }

    public string Text => _text.Text;

    public void Dispose()
    {
        _text.ValueChanged -= _onChanged;
        _text.Dispose();
        TextChanged = null;
    }

    public List<string> WordsLingoCodeKeywords = [.. _lingoKeyWordsDefault];
    private static readonly string[] _lingoKeyWordsDefault = [
        "Property", "On", "If", "Then", "Else", "Me", "Or", "And", "True", "False", "Repeat", "With", "End", "To", "Return", "While", "The", "New", "Global"];

    public List<string> WordsLingoCodeBuiltIn = [.. _lingoWordsDefault];
    private static readonly string[] _lingoWordsDefault = [
            "Point","Loc","Void","Char","Rgb","In","Line","Word",
            "Default","Format","Color","Comment","Integer","Boolean","String","Text","String","Symbol",
            "GetPropertyDescriptionList","GetBehaviorTooltip","IsOKToAttach","GetBehaviorDescription",
            "_Movie","ActorList","Cursor","Alert",
            "MemberNum","Member","Preload","Sound",
            "Sprite","SpriteNum","LocH","LocV","LocZ","Blend","Ink","MouseH","MouseV","Puppet",
            "DeleteOne","Append","GetPos","AddProp","SendSprite","VoidP","Frame","Length","Count",
            "Go","Exit",
            "Value",
            "Script","Handler",
            "StepFrame","BeginSprite","EndSprite",
            "StartMovie","StopMovie",
            "_Mouse","MouseUp","MouseDown","MouseEnter","MouseLeave",
            "NetError","NetTextResult","GetNetText",
            "_Key","KeyPressed","ControlDown","ShiftDown",
            "_Player",
            "_Sound",
            "_System",
            "Channel","Number",
            "CastLib",
            "Put",
        ];

    public List<string> WordsCCharpCodeBuiltIn = [.. _csharpWordsDefault];
    private static readonly string[] _csharpWordsDefault = [
             "abstract","as","base","break","case","catch","checked","class","const","continue",
            "default","delegate","do","else","enum","event","explicit","extern","finally","fixed",
            "for","foreach","goto","if","implicit","in","interface","internal","is","lock",
            "namespace","new","operator","out","override","params","private","protected","public",
            "readonly","record","ref","return","sealed","sizeof","stackalloc","static","struct",
            "switch","this","throw","try","typeof","unsafe","using","virtual","volatile","while",
            "async","await","var","null","true","false","void",
        ];

    public List<string> WordsCCharpCodeTypes = [.. _csharpWordsCodeTypesDefault];
    private static readonly string[] _csharpWordsCodeTypesDefault = [
            "bool","byte","sbyte","char","decimal","double","float","int","uint","nint","nuint",
            "long","ulong","object","short","ushort","string","void",
        // NET
        "List","Add","Remove","IndexOf",
        // lingo
         "APoint","ARect","AColor","Loc","Char","FromHex","Line","Word",
            "format","AColor","Comment","Text","Symbol",
            "GetPropertyDescriptionList","GetBehaviorTooltip","IsOKToAttach","GetBehaviorDescription","BehaviorPropertyDescriptionList",
            "_Movie","Actorlist","Cursor","Alert",
            "MemberNum","Member","Preload","Sound",
            "Sprite","SpriteNum","LocH","LocV","LocZ","Blend","Ink","MouseH","MouseV","Puppet","ILingoSprite",
            "DeleteOne","Append","GetPos","Deleteone","Addprop","Sendsprite","Frame","Length","Count",
            "Go","GoTo",
            "Script","Handler",
            "StepFrame","BeginSprite","EndSprite",
            "StartMovie","StopMovie",
            "_Mouse","MouseUp","MouseDown","MouseEnter","MouseLeave","LingoMouseEvent",
            "NetError","NetTextResult","GetNetText",
            "_Key","KeyPressed","ControlDown","ShiftDown","LingoKeyEvent",
            "_Player",
            "_Sound",
            "_System",
            "Channel","Number",
            "CastLib","LingoCast","LingoInkType","ILingoMember"
        ];

    public static IEnumerable<string> CaseInsensitiveWords(IEnumerable<string> words)
        => words.SelectMany(w => new[] { w, w.ToLowerInvariant() }).Distinct();
}
