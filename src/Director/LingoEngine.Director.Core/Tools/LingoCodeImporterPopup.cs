using System;
using System.IO;
using System.Linq;
using AbstUI.Components;
using AbstUI.Components.Containers;
using AbstUI.Components.Inputs;
using AbstUI.Inputs;
using AbstUI.Primitives;
using AbstUI.Commands;
using AbstUI.Windowing;
using LingoEngine.Director.Core.FileSystems;
using LingoEngine.Director.Core.Tools.Commands;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Lingo.Core;
using TextCopy;

namespace LingoEngine.Director.Core.Tools;

public class LingoCodeImporterPopupHandler : IAbstCommandHandler<OpenLingoCodeImporterCommand>
{
    private readonly IAbstWindowManager _windowManager;
    private readonly IAbstComponentFactory _componentFactory;

    public LingoCodeImporterPopupHandler(IAbstWindowManager windowManager, IAbstComponentFactory componentFactory)
    {
        _windowManager = windowManager;
        _componentFactory = componentFactory;
    }

    public bool CanExecute(OpenLingoCodeImporterCommand command) => true;

    public bool Handle(OpenLingoCodeImporterCommand command)
    {
        var component = (LingoCodeImporterPopup)_componentFactory.CreateElement<LingoCodeImporterPopup, IAbstDialog>();
        _windowManager.ShowCustomDialog("Lingo code importer", component.GetFWPanel(), component);
        return true;
    }
}

public class LingoCodeImporterPopup : AbstDialog
{
    protected readonly ILingoFrameworkFactory _factory;
    private readonly IDirFolderPicker _folderPicker;
    private readonly AbstPanel _panel;
    private AbstWrapPanel _folderPanel = null!;
    private AbstWrapPanel _mainPanel = null!;
    private AbstItemList _fileList = null!;
    private DirCodeHighlichter _lingoHighlighter = null!;
    private DirCodeHighlichter _csharpHighlighter = null!;
    private AbstInputText _errorInput = null!;
    private string _currentFolder = string.Empty;

    protected sealed class ViewModel
    {
        public string Lingo { get; set; } = string.Empty;
        public string CSharp { get; set; } = string.Empty;
        public string Errors { get; set; } = string.Empty;
    }

    public LingoCodeImporterPopup(ILingoFrameworkFactory factory, IDirFolderPicker folderPicker, IAbstGlobalMouse mouse, IAbstGlobalKey key)
        : base(mouse, key)
    {
        _factory = factory;
        _folderPicker = folderPicker;
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
        var root = _factory.CreatePanel("LingoImporterRoot");
        root.Width = 1000;
        root.Height = 560;

        _folderPanel = _factory.CreateWrapPanel(AOrientation.Horizontal, "FolderSelect");
        _folderPanel.Width = 1000;
        _folderPanel.Height = 40;
        var folderInput = _factory.CreateInputText("FolderInput", 0, null);
        folderInput.Width = 800;
        folderInput.Height = 30;
        _folderPanel.AddItem(folderInput);
        var browse = _factory.CreateButton("BrowseButton", "Browse");
        browse.Pressed += () => _folderPicker.PickFolder(path =>
        {
            folderInput.Text = path;
            ShowFiles(path, vm);
        });
        _folderPanel.AddItem(browse);
        var open = _factory.CreateButton("OpenButton", "Open");
        open.Pressed += () => ShowFiles(folderInput.Text, vm);
        _folderPanel.AddItem(open);
        root.AddItem(_folderPanel);

        _mainPanel = _factory.CreateWrapPanel(AOrientation.Horizontal, "MainContent");
        _mainPanel.Width = 1000;
        _mainPanel.Height = 520;
        _mainPanel.Margin = new AMargin(0, 40, 0, 0);
        _mainPanel.Visibility = false;
        root.AddItem(_mainPanel);

        _fileList = _factory.CreateItemList("FilesList", key =>
        {
            if (string.IsNullOrEmpty(key)) return;
            LoadFile(key, vm);
        });
        _fileList.Width = 200;
        _fileList.Height = 520;
        _mainPanel.AddItem(_fileList);

        var converterPanel = BuildConverterArea(vm);
        _mainPanel.AddItem(converterPanel);

        return root;
    }

    private AbstPanel BuildConverterArea(ViewModel vm)
    {
        var container = _factory.CreatePanel("ConverterPanel");
        container.Width = 800;
        container.Height = 520;

        var content = _factory.CreateWrapPanel(AOrientation.Horizontal, "Content");
        content.Width = 800;
        content.Height = 460;
        container.AddItem(content);

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

        _lingoHighlighter = _factory.ComponentFactory.CreateElement<DirCodeHighlichter>();
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

        _errorInput = _factory.CreateInputText("ErrorsText", 0, null);
        _errorInput.Width = 800;
        _errorInput.Height = 60;
        _errorInput.IsMultiLine = true;
        _errorInput.Enabled = false;
        _errorInput.Margin = new AMargin(0, 500, 0, 0);
        container.AddItem(_errorInput, 0, 450);

        var menuBar = _factory.CreateWrapPanel(AOrientation.Horizontal, "BottomBar");
        menuBar.Width = 800;
        menuBar.Height = 40;
        menuBar.Margin = new AMargin(0, 520, 0, 0);
        container.AddItem(menuBar);

        menuBar.ComposeForToolBar()
            .AddButton("ConvertButton", "Convert", () => Convert(vm));

        return container;
    }

    private void ShowFiles(string folder, ViewModel vm)
    {
        if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder)) return;
        _currentFolder = folder;
        _fileList.ClearItems();
        foreach (var file in Directory.GetFiles(folder, "*.ls", SearchOption.AllDirectories))
        {
            _fileList.AddItem(file, Path.GetFileName(file));
        }
        _folderPanel.Visibility = false;
        _mainPanel.Visibility = true;
    }

    private void LoadFile(string path, ViewModel vm)
    {
        if (!File.Exists(path)) return;
        var text = File.ReadAllText(path);
        _lingoHighlighter.SetText(text);
        vm.Lingo = text;
        Convert(vm);
    }

    private void Convert(ViewModel vm)
    {
        var converter = new LingoToCSharpConverter();
        vm.CSharp = converter.Convert(vm.Lingo);
        _csharpHighlighter.SetText(vm.CSharp);
        vm.Errors = string.Join("\n", converter.Errors.Select(e =>
            string.IsNullOrEmpty(e.File)
                ? $"Line {e.LineNumber}: {e.LineText} - {e.Error}"
                : $"{e.File}:{e.LineNumber}: {e.LineText} - {e.Error}"));
        _errorInput.Text = vm.Errors;
    }

    public void Dispose()
    {
        _lingoHighlighter.Dispose();
        _csharpHighlighter.Dispose();
    }
}
