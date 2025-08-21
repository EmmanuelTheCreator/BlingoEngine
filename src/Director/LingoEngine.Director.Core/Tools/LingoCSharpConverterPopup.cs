using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.Commands;
using LingoEngine.Director.Core.Tools.Commands;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Lingo.Core;
using TextCopy;
using AbstUI.Windowing;
using AbstUI.Inputs;
using AbstUI.Components.Containers;

namespace LingoEngine.Director.Core.Tools;

public class LingoCSharpConverterPopupHandler : IAbstCommandHandler<OpenLingoCSharpConverterCommand> 
{
    private readonly IAbstWindowManager _windowManager;
    private readonly IAbstComponentFactory _componentFactory;

    public LingoCSharpConverterPopupHandler(IAbstWindowManager windowManager, IAbstComponentFactory componentFactory)
    {
        _windowManager = windowManager;
        _componentFactory = componentFactory;
    }
    public bool CanExecute(OpenLingoCSharpConverterCommand command) => true;

    public virtual bool Handle(OpenLingoCSharpConverterCommand command)
    {
        var component = (LingoCSharpConverterPopup)_componentFactory.CreateElement<LingoCSharpConverterPopup, IAbstDialog>();
        
        _windowManager.ShowCustomDialog("Lingo to C#", component.GetFWPanel(), component);
        return true;
    }
}
public class LingoCSharpConverterPopup : AbstDialog
{
    protected readonly ILingoFrameworkFactory _factory;
    private readonly AbstPanel _panel;
    private DirCodeHighlichter _csharpHighlighter = null!;
    private DirCodeHighlichter _lingoHighlighter = null!;

    protected sealed class ViewModel
    {
        public string Lingo { get; set; } = string.Empty;
        public string CSharp { get; set; } = string.Empty;
        public string Errors { get; set; } = string.Empty;
    }

    public LingoCSharpConverterPopup(ILingoFrameworkFactory factory, IAbstGlobalMouse mouse, IAbstGlobalKey key)
        :base(mouse,key)
    {
        _factory = factory;
        var vm = new ViewModel();
        _panel = BuildPanel(vm);
    }

  

    public override void Init(IAbstFrameworkDialog framework) 
    {
        base.Init(framework);
        _lingoHighlighter.Update();
        _csharpHighlighter.Update();
    }
    internal IAbstFrameworkPanel GetFWPanel() => _panel.Framework<IAbstFrameworkPanel>();
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

        _lingoHighlighter = _factory.ComponentFactory.CreateElement<DirCodeHighlichter>(); // new DirCodeHighlichter(_factory, DirCodeHighlichter.Language.Lingo);
        _lingoHighlighter.CodeLanguage = DirCodeHighlichter.SourceCodeLanguage.Lingo;
        _lingoHighlighter.Width = 380;
        _lingoHighlighter.Height = 420;
        _lingoHighlighter.TextChanged += () => vm.Lingo = _lingoHighlighter.Text;
        
        left.AddItem(_lingoHighlighter.TextComponent);

        var rightHeader = _factory.CreateWrapPanel(AOrientation.Horizontal, "CSharpHeader");
        right.AddItem(rightHeader);
        rightHeader.Compose()
            .AddLabel("CSharpLabel", "C#")
            .AddButton("CopyCSharp", "Copy", () => ClipboardService.SetText(vm.CSharp));

        _csharpHighlighter = _factory.ComponentFactory.CreateElement<DirCodeHighlichter>();
        _csharpHighlighter.CodeLanguage = DirCodeHighlichter.SourceCodeLanguage.CSharp;
        _csharpHighlighter.Width = 380;
        _csharpHighlighter.Height = 420;
        _csharpHighlighter.TextChanged += () => vm.CSharp = _csharpHighlighter.Text;
        
        right.AddItem(_csharpHighlighter.TextComponent);

        var errorInput = _factory.CreateInputText("ErrorsText", 0, null);
        errorInput.Width = 800;
        errorInput.Height = 60;
        errorInput.IsMultiLine = true;
        errorInput.Enabled = false;
        errorInput.Margin = new AMargin(0, 500, 0, 0);
        //prop?.SetValue(framework, DirectorColors.Notification_Error_Border);
        root.AddItem(errorInput, 0, 450);

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
                _csharpHighlighter.SetText(vm.CSharp);
                vm.Errors = string.Join("\n", converter.Errors.Select(e =>
                    string.IsNullOrEmpty(e.File)
                        ? $"Line {e.LineNumber}: {e.LineText} - {e.Error}"
                        : $"{e.File}:{e.LineNumber}: {e.LineText} - {e.Error}"));
                errorInput.Text = vm.Errors;
            });

        return root;
    }

    public void Dispose()
    {
        _lingoHighlighter.Dispose();
        _csharpHighlighter.Dispose();
    }

   

    public DirCodeHighlichter LingoHighlighter => _lingoHighlighter;
    public DirCodeHighlichter CSharpHighlighter => _csharpHighlighter;
}
