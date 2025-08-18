using AbstUI.Components;
using AbstUI.Primitives;
using LingoEngine.Commands;
using LingoEngine.Director.Core.Tools.Commands;
using LingoEngine.Director.Core.UI;
using LingoEngine.Director.Core.Windowing;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Lingo.Core;
using LingoEngine.Texts;
using TextCopy;

namespace LingoEngine.Director.Core.Tools;

public interface ILingoFrameworkCSharpConverterPopup : IDirFrameworkDialog
{
    public void UpdateLingoColors(AbstInputText inputText);
    public void UpdateCSharpColors(AbstInputText inputText);
}
public class LingoCSharpConverterPopup : ICommandHandler<OpenLingoCSharpConverterCommand>, ILingoDialog
{
    protected readonly IDirectorWindowManager _windowManager;
    protected readonly ILingoFrameworkFactory _factory;
    private ILingoFrameworkCSharpConverterPopup? _frameworkObj;
    private AbstInputText _csharpInput;
    private AbstInputText _lingoInput;

    protected sealed class ViewModel
    {
        public string Lingo { get; set; } = string.Empty;
        public string CSharp { get; set; } = string.Empty;
        public string Errors { get; set; } = string.Empty;
    }

    public LingoCSharpConverterPopup(IDirectorWindowManager windowManager, ILingoFrameworkFactory factory)
    {
        _windowManager = windowManager;
        _factory = factory;
    }

    public bool CanExecute(OpenLingoCSharpConverterCommand command) => true;

    public virtual bool Handle(OpenLingoCSharpConverterCommand command)
    {
        var vm = new ViewModel();
        var panel = BuildPanel(vm);
        _windowManager.ShowCustomDialog<LingoCSharpConverterPopup>("Lingo to C#", panel.Framework<IAbstFrameworkPanel>(), this);
        return true;
    }

    public void Init(IDirFrameworkDialog framework)
    {
        _frameworkObj = (ILingoFrameworkCSharpConverterPopup)framework;
    }

    protected AbstPanel BuildPanel(ViewModel vm)
    {
        var root = _factory.CreatePanel("LingoCSharpRoot");
        root.Width = 800;
        root.Height = 560;

        var content = _factory.CreateWrapPanel(AOrientation.Horizontal, "Content");
        content.Width = 800;
        content.Height = 460;
        root.AddItem(content);

        var left = _factory.CreateWrapPanel(AOrientation.Vertical, "LingoColumn");
        left.Width = 400;
        left.Height = 460;
        content.AddItem(left);

        var right = _factory.CreateWrapPanel(AOrientation.Vertical, "CSharpColumn");
        right.Width = 400;
        right.Height = 460;
        content.AddItem(right);

        var leftHeader = _factory.CreateWrapPanel(AOrientation.Horizontal, "LingoHeader");
        left.AddItem(leftHeader);
        leftHeader.Compose()
            .AddLabel("LingoLabel", "Lingo")
            .AddButton("CopyLingo", "Copy", () => ClipboardService.SetText(vm.Lingo));

        _lingoInput = _factory.CreateInputText("LingoText", 0, text => vm.Lingo = text);
        _lingoInput.Width = 380;
        _lingoInput.Height = 420;
        _lingoInput.IsMultiLine = true;
        _lingoInput.ValueChanged += LingoInput_ValueChanged;
        left.AddItem(_lingoInput);

        var rightHeader = _factory.CreateWrapPanel(AOrientation.Horizontal, "CSharpHeader");
        right.AddItem(rightHeader);
        rightHeader.Compose()
            .AddLabel("CSharpLabel", "C#")
            .AddButton("CopyCSharp", "Copy", () => ClipboardService.SetText(vm.CSharp));

        _csharpInput = _factory.CreateInputText("CSharpText", 0, null);
        _csharpInput.Width = 380;
        _csharpInput.Height = 420;
        //csharpInput.Enabled = false;
        _csharpInput.IsMultiLine = true;
        _csharpInput.ValueChanged += CsharpInput_ValueChanged;
        right.AddItem(_csharpInput);

        var errorInput = _factory.CreateInputText("ErrorsText", 0, null);
        errorInput.Width = 800;
        errorInput.Height = 60;
        errorInput.IsMultiLine = true;
        errorInput.Enabled = false;
        errorInput.Margin = new AMargin(0, 500, 0, 0);
        //prop?.SetValue(framework, DirectorColors.Notification_Error_Border);
        root.AddItem(errorInput,0,450);
        var framework = errorInput.FrameworkObj;

        var menuBar = _factory.CreateWrapPanel(AOrientation.Horizontal, "BottomBar");
        menuBar.Width = 800;
        menuBar.Height = 40;
        menuBar.Margin = new AMargin(0, 520, 0, 0);
        root.AddItem(menuBar); // _factory.CreateLayoutWrapper(menuBar, 0, 860));

        menuBar.ComposeForToolBar()
            .AddButton("ConvertButton", "Convert", () =>
            {
                var converter = new LingoToCSharpConverter();
                vm.CSharp = converter.Convert(vm.Lingo);
                _csharpInput.Text = vm.CSharp; //.Replace("\r", "\n");
                vm.Errors = string.Join("\n", converter.Errors.Select(e =>
                    string.IsNullOrEmpty(e.File)
                        ? $"Line {e.LineNumber}: {e.LineText} - {e.Error}"
                        : $"{e.File}:{e.LineNumber}: {e.LineText} - {e.Error}"));
                errorInput.Text = vm.Errors;
                _frameworkObj?.UpdateCSharpColors(_csharpInput);
            });

        return root;
    }

    private void CsharpInput_ValueChanged() => _frameworkObj?.UpdateCSharpColors(_csharpInput);

    private void LingoInput_ValueChanged() => _frameworkObj?.UpdateLingoColors(_lingoInput);

    public void Dispose()
    {
        _lingoInput.ValueChanged -= LingoInput_ValueChanged;
        _csharpInput.ValueChanged -= CsharpInput_ValueChanged;
        _lingoInput.Dispose();
        _csharpInput.Dispose();
    }

    public List<string> WordsLingoCodeKeywords = [.. _lingoKeyWordsDefault];
    private static readonly string[] _lingoKeyWordsDefault = [
        "property", "on", "if", "then", "else", "me", "or", "and", "true", "false", "repeat", "with", "end", "to", "return", "while", "the", "new", "global"];

    public List<string> WordsLingoCodeBuiltIn = [.. _lingoWordsDefault];
    private static readonly string[] _lingoWordsDefault = [
            "point","loc","void","char","rgb","in","line","word",
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
            "_Mouse","mouseup","mousedown","mouseenter","mouseleave",
            "neterror","nettextresult","getNetText",
            "_key","keypressed","controldown","shiftdown",
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
}
