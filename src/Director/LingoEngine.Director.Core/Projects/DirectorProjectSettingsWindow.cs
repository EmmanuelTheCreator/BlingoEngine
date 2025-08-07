using LingoEngine.Director.Core.FileSystems;
using LingoEngine.Director.Core.UI;
using LingoEngine.Director.Core.Windowing;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Gfx;
using LingoEngine.Primitives;

namespace LingoEngine.Director.Core.Projects;

/// <summary>
/// Framework independent implementation of the project settings window.
/// </summary>
public class DirectorProjectSettingsWindow : DirectorWindow<IDirFrameworkProjectSettingsWindow>
{
    private readonly ProjectSettingsEditorState _state;
    private readonly IIdePathResolver _resolver;
    private readonly IDirFolderPicker _picker;
    private readonly IDirectorWindowManager _windowManager;

    private readonly LingoGfxWrapPanel _root;
    private LingoGfxWrapPanel? _vsCodePathRow;
    private LingoGfxWrapPanel? _vsPathRow;
    private LingoGfxLabel? _slnPreviewLabel;
    private string _projectName = "";
    private string _folderName = "";
    private List<KeyValuePair<string, string>> _ideTypes = [
        new KeyValuePair<string, string>(DirectorIdeType.VisualStudio.ToString(), "Visual Studio"),
        new KeyValuePair<string, string>(DirectorIdeType.VisualStudioCode.ToString(), "Visual Studio Code"),
        ];
    private string visualStudioPath = "";
    private string visualStudioCodePath = "";
    private LingoGfxInputText? _VsPathEdit;
    private LingoGfxInputText? _VSCodePathEdit;

    public LingoGfxWrapPanel RootPanel => _root;
    public string FolderName
    {
        get => _folderName;
        set
        {
            _folderName = value;
            UpdateSlnPreview();
        }
    }
    public string SolutionPreviewName { get; set; } = "";
    public string VisualStudioPath
    {
        get => visualStudioPath; set
        {
            visualStudioPath = value;
            if (_VsPathEdit != null)
                _VsPathEdit.Text = visualStudioPath;
        }
    }
    public string VisualStudioCodePath
    {
        get => visualStudioCodePath; set
        {
            visualStudioCodePath = value;
            if (_VSCodePathEdit != null)
                _VSCodePathEdit.Text = visualStudioCodePath;
        }
    }
    public string ProjectName
    {
        get => _projectName;
        set
        {
            _projectName = value;
            UpdateSlnPreview();
        }
    }
    public DirectorIdeType SelectedIde { get; set; }
    public int Width { get; set; } = 500;
    public int Height { get; set; } = 200;

    public DirectorProjectSettingsWindow(
        ProjectSettingsEditorState state,
        IIdePathResolver resolver,
        IDirFolderPicker picker,
        IDirectorWindowManager windowManager,
        ILingoFrameworkFactory factory) : base(factory)
    {
        _state = state;
        _resolver = resolver;
        _picker = picker;
        _windowManager = windowManager;
        LoadState(state);

        _root = factory.CreateWrapPanel(LingoOrientation.Vertical, "ProjectSettingsRoot");
        _root.ItemMargin = new LingoPoint(0, 4);
        _root.Width = Width;
        _root.Height = Height;

        // Project name
        _root.Compose()
            .NewLine("NameRow")
            .AddLabel("NameLabel", "Project Name:", 11, 100)
            .AddTextInput("NameEdit", this, s => s.ProjectName, 200)
            // Project folder
            .NewLine("FolderRow")
            .AddLabel("FolderLabel", "Project Folder:", 11, 100)
            .AddTextInput("FolderEdit", this, s => s.FolderName, 200)
             // Preview
            .NewLine("PeviewRow")
            .AddLabel("PreviewLabel", GetSlnPreview(), 11, null, c => { _slnPreviewLabel = c; })
            // IDE selection
             .NewLine("IdeRow")
            .AddLabel("IdeLabel", "IDE", 11, 100)
            .AddCombobox("ComboIdeTypes", _ideTypes, 100, state.SelectedIde.ToString(), s => { SelectedIde = Enum.Parse<DirectorIdeType>(s!); UpdateIdePathVisibility(); })
            // Visual Studio path
            .NewLine("VsPathRow")
            .Configure(c => _vsPathRow = c)
            .AddLabel("VsPathLabel", "VS Path", 11, 100)
            .AddTextInput("VsPathEdit", this, s => s.VisualStudioPath, 200,c => _VsPathEdit = c)
            .AddButton("VsBrowse", "Browse...", () => _picker.PickFolder(path => VisualStudioPath = path), c => c.Width = 80)
            .AddButton("VsAuto", "Auto", () => VisualStudioPath = _resolver.AutoDetectVisualStudioPath() ?? string.Empty, c => c.Width = 80)
            // VS Code path
            .NewLine("VSCodePathRow")
            .Configure(c => _vsCodePathRow = c)
            .AddLabel("VSCodePathLabel", "VS Code Path", 11, 100)
            .AddTextInput("VSCodePathEdit", this, s => s.VisualStudioCodePath, 200, c => _VSCodePathEdit = c)
            .AddButton("CodeBrowse", "Browse...", () => _picker.PickFolder(path => VisualStudioCodePath = path), c => c.Width = 80)
            .AddButton("CodeAuto", "Auto", () => VisualStudioCodePath = _resolver.AutoDetectVSCodePath() ?? string.Empty, c => c.Width = 80)
            // Save & Apply buttons
            .NewLine("ButtonRow")
            .AddButton("SaveButton", "Save", OnSavePressed, c => c.Width = 90)
            .AddButton("ApplyButton", "Apply", () =>
            {
                if (ValidateSettings())
                    SaveState();
            }, c => c.Width = 90)
            .Finalize();

        UpdateIdePathVisibility();
    }

   

   
    private void UpdateSlnPreview()
    {
        SolutionPreviewName = GetSlnPreview();
        _slnPreviewLabel!.Text = SolutionPreviewName;
    }
    private string GetSlnPreview()
    {
        var name = ProjectName.Trim();
        var folder = FolderName.Trim();
        return string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(folder)
            ? "(Solution path preview unavailable)"
            : $"Solution: {System.IO.Path.Combine(folder, $"{name}.sln")}";
    }

    private void OnSavePressed()
    {
        if (!ValidateSettings())
            return;

        SaveState();
        CloseWindow();
    }
    private void LoadState(ProjectSettingsEditorState state)
    {
        _projectName = state.ProjectName;
        _folderName = state.ProjectFolder;
        SelectedIde = state.SelectedIde;
        VisualStudioPath = state.VisualStudioPath;
        VisualStudioCodePath = state.VisualStudioCodePath;
    }
    private void SaveState()
    {
        _state.ProjectName = ProjectName.Trim();
        _state.ProjectFolder = FolderName.Trim();
        _state.SelectedIde = SelectedIde;
        _state.VisualStudioPath = VisualStudioPath.Trim();
        _state.VisualStudioCodePath = VisualStudioCodePath.Trim();
    }

    private void UpdateIdePathVisibility()
    {
        var showVs = SelectedIde == DirectorIdeType.VisualStudio;
        _vsPathRow!.Visibility = showVs;
        _vsCodePathRow!.Visibility = !showVs;
    }

    private bool ValidateSettings()
    {
        if (string.IsNullOrWhiteSpace(ProjectName))
        {
            _windowManager.ShowNotification("Project name is required.", DirUINotificationType.Error);
            return false;
        }

        if (string.IsNullOrWhiteSpace(FolderName))
        {
            _windowManager.ShowNotification("Project folder is required.", DirUINotificationType.Error);
            return false;
        }

        if (SelectedIde == DirectorIdeType.VisualStudio && string.IsNullOrWhiteSpace(VisualStudioPath))
        {
            _windowManager.ShowNotification("Visual Studio path is required.", DirUINotificationType.Error);
            return false;
        }

        if (SelectedIde == DirectorIdeType.VisualStudioCode && string.IsNullOrWhiteSpace(VisualStudioCodePath))
        {
            _windowManager.ShowNotification("VS Code path is required.", DirUINotificationType.Error);
            return false;
        }

        return true;
    }
}

