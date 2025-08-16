using AbstUI.Components;
using AbstUI.Primitives;
using LingoEngine.Commands;
using LingoEngine.Director.Core.Tools.Commands;
using LingoEngine.Director.Core.UI;
using LingoEngine.Director.Core.Windowing;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Lingo.Core;
using TextCopy;

namespace LingoEngine.Director.Core.Tools;

public class LingoCSharpConverterPopup : ICommandHandler<OpenLingoCSharpConverterCommand>
{
    private readonly IDirectorWindowManager _windowManager;
    private readonly ILingoFrameworkFactory _factory;

    private sealed class ViewModel
    {
        public string Lingo { get; set; } = string.Empty;
        public string CSharp { get; set; } = string.Empty;
    }

    public LingoCSharpConverterPopup(IDirectorWindowManager windowManager, ILingoFrameworkFactory factory)
    {
        _windowManager = windowManager;
        _factory = factory;
    }

    public bool CanExecute(OpenLingoCSharpConverterCommand command) => true;

    public bool Handle(OpenLingoCSharpConverterCommand command)
    {
        var vm = new ViewModel();
        var panel = BuildPanel(vm);
        _windowManager.ShowCustomDialog("Lingo to C#", panel.Framework<IAbstFrameworkPanel>());
        return true;
    }

    private AbstPanel BuildPanel(ViewModel vm)
    {
        var root = _factory.CreatePanel("LingoCSharpRoot");
        root.Width = 800;
        root.Height = 900;

        var content = _factory.CreateWrapPanel(AOrientation.Horizontal, "Content");
        content.Width = 800;
        content.Height = 860;
        root.AddItem(content);

        var left = _factory.CreateWrapPanel(AOrientation.Vertical, "LingoColumn");
        left.Width = 400;
        left.Height = 860;
        content.AddItem(left);

        var right = _factory.CreateWrapPanel(AOrientation.Vertical, "CSharpColumn");
        right.Width = 400;
        right.Height = 860;
        content.AddItem(right);

        var leftHeader = _factory.CreateWrapPanel(AOrientation.Horizontal, "LingoHeader");
        left.AddItem(leftHeader);
        leftHeader.Compose()
            .AddLabel("LingoLabel", "Lingo")
            .AddButton("CopyLingo", "Copy", () => ClipboardService.SetText(vm.Lingo));

        var lingoInput = _factory.CreateInputText("LingoText", 0, text => vm.Lingo = text);
        lingoInput.Width = 380;
        lingoInput.Height = 820;
        lingoInput.IsMultiLine = true;
        left.AddItem(lingoInput);

        var rightHeader = _factory.CreateWrapPanel(AOrientation.Horizontal, "CSharpHeader");
        right.AddItem(rightHeader);
        rightHeader.Compose()
            .AddLabel("CSharpLabel", "C#")
            .AddButton("CopyCSharp", "Copy", () => ClipboardService.SetText(vm.CSharp));

        var csharpInput = _factory.CreateInputText("CSharpText", 0, null);
        csharpInput.Width = 380;
        csharpInput.Height = 820;
        //csharpInput.Enabled = false;
        csharpInput.IsMultiLine = true;
        right.AddItem(csharpInput);

        var menuBar = _factory.CreateWrapPanel(AOrientation.Horizontal, "BottomBar");
        menuBar.Width = 800;
        menuBar.Height = 40;
        menuBar.Margin = new AMargin(0, 860, 0, 0);
        root.AddItem(menuBar); // _factory.CreateLayoutWrapper(menuBar, 0, 860));

        menuBar.ComposeForToolBar()
            .AddButton("ConvertButton", "Convert", () =>
            {
                vm.CSharp = LingoToCSharpConverter.Convert(vm.Lingo);
                csharpInput.Text = vm.CSharp.Replace("\r", "\n");
            });

        return root;
    }
}
