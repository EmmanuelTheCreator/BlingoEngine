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
    private readonly IExecutableFilePicker _picker;
    private readonly IDirectorWindowManager _windowManager;

    private readonly LingoGfxWrapPanel _root;
    private readonly LingoGfxInputText _nameEdit;
    private readonly LingoGfxInputText _folderEdit;
    private readonly LingoGfxInputCombobox _ideSelect;
    private readonly LingoGfxWrapPanel _vsPathRow;
    private readonly LingoGfxInputText _vsPathEdit;
    private readonly LingoGfxWrapPanel _vsCodePathRow;
    private readonly LingoGfxInputText _vsCodePathEdit;
    private readonly LingoGfxLabel _slnPreviewLabel;

    public LingoGfxWrapPanel RootPanel => _root;

    public DirectorProjectSettingsWindow(
        ProjectSettingsEditorState state,
        IIdePathResolver resolver,
        IExecutableFilePicker picker,
        IDirectorWindowManager windowManager,
        ILingoFrameworkFactory factory) : base(factory)
    {
        _state = state;
        _resolver = resolver;
        _picker = picker;
        _windowManager = windowManager;

        _root = factory.CreateWrapPanel(LingoOrientation.Vertical, "ProjectSettingsRoot");
        _root.ItemMargin = new LingoPoint(0, 4);

        // Project name
        var nameRow = factory.CreateWrapPanel(LingoOrientation.Horizontal, "NameRow");
        var nameLabel = factory.CreateLabel("NameLabel", "Project Name");
        nameLabel.Width = 100;
        _nameEdit = factory.CreateInputText("NameEdit");
        _nameEdit.Text = state.ProjectName;
        _nameEdit.ValueChanged += () => _slnPreviewLabel!.Text = GetSlnPreview();
        nameRow.AddItem(nameLabel);
        nameRow.AddItem(_nameEdit);
        _root.AddItem(nameRow);

        // Project folder
        var folderRow = factory.CreateWrapPanel(LingoOrientation.Horizontal, "FolderRow");
        var folderLabel = factory.CreateLabel("FolderLabel", "Project Folder");
        folderLabel.Width = 100;
        _folderEdit = factory.CreateInputText("FolderEdit");
        _folderEdit.Text = state.ProjectFolder;
        _folderEdit.ValueChanged += () => _slnPreviewLabel!.Text = GetSlnPreview();
        folderRow.AddItem(folderLabel);
        folderRow.AddItem(_folderEdit);
        _root.AddItem(folderRow);

        _slnPreviewLabel = factory.CreateLabel("SlnPreview", GetSlnPreview());
        _root.AddItem(_slnPreviewLabel);

        // IDE selection
        var ideRow = factory.CreateWrapPanel(LingoOrientation.Horizontal, "IdeRow");
        var ideLabel = factory.CreateLabel("IdeLabel", "IDE");
        ideLabel.Width = 100;
        _ideSelect = factory.CreateInputCombobox("IdeSelect");
        _ideSelect.AddItem(((int)DirectorIdeType.VisualStudio).ToString(), "Visual Studio");
        _ideSelect.AddItem(((int)DirectorIdeType.VisualStudioCode).ToString(), "Visual Studio Code");
        _ideSelect.SelectedKey = ((int)state.SelectedIde).ToString();
        _ideSelect.ValueChanged += UpdateIdePathVisibility;
        ideRow.AddItem(ideLabel);
        ideRow.AddItem(_ideSelect);
        _root.AddItem(ideRow);

        // Visual Studio path
        _vsPathRow = factory.CreateWrapPanel(LingoOrientation.Horizontal, "VsPathRow");
        var vsLabel = factory.CreateLabel("VsPathLabel", "VS Path");
        vsLabel.Width = 100;
        _vsPathEdit = factory.CreateInputText("VsPathEdit");
        _vsPathEdit.Text = state.VisualStudioPath;
        var vsBrowse = factory.CreateButton("VsBrowse", "...");
        vsBrowse.Pressed += () => _picker.PickExecutable(path => _vsPathEdit.Text = path);
        var vsAuto = factory.CreateButton("VsAuto", "Auto");
        vsAuto.Pressed += () => _vsPathEdit.Text = _resolver.AutoDetectVisualStudioPath() ?? string.Empty;
        _vsPathRow.AddItem(vsLabel);
        _vsPathRow.AddItem(_vsPathEdit);
        _vsPathRow.AddItem(vsBrowse);
        _vsPathRow.AddItem(vsAuto);
        _root.AddItem(_vsPathRow);

        // VS Code path
        _vsCodePathRow = factory.CreateWrapPanel(LingoOrientation.Horizontal, "VSCodePathRow");
        var codeLabel = factory.CreateLabel("VSCodePathLabel", "VS Code Path");
        codeLabel.Width = 100;
        _vsCodePathEdit = factory.CreateInputText("VSCodePathEdit");
        _vsCodePathEdit.Text = state.VisualStudioCodePath;
        var codeBrowse = factory.CreateButton("CodeBrowse", "...");
        codeBrowse.Pressed += () => _picker.PickExecutable(path => _vsCodePathEdit.Text = path);
        var codeAuto = factory.CreateButton("CodeAuto", "Auto");
        codeAuto.Pressed += () => _vsCodePathEdit.Text = _resolver.AutoDetectVSCodePath() ?? string.Empty;
        _vsCodePathRow.AddItem(codeLabel);
        _vsCodePathRow.AddItem(_vsCodePathEdit);
        _vsCodePathRow.AddItem(codeBrowse);
        _vsCodePathRow.AddItem(codeAuto);
        _root.AddItem(_vsCodePathRow);

        // Save & Apply buttons
        _root.Compose(factory)
            .AddButton("SaveButton", "Save", OnSavePressed)
            .AddButton("ApplyButton", "Apply", () =>
            {
                if (ValidateSettings())
                    SaveState();
            })
            .Finalize();

        UpdateIdePathVisibility();
    }

    private DirectorIdeType SelectedIde
        => (DirectorIdeType)int.Parse(_ideSelect.SelectedKey ?? "0");

    private string GetSlnPreview()
    {
        var name = _nameEdit.Text.Trim();
        var folder = _folderEdit.Text.Trim();
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

    private void SaveState()
    {
        _state.ProjectName = _nameEdit.Text.Trim();
        _state.ProjectFolder = _folderEdit.Text.Trim();
        _state.SelectedIde = SelectedIde;
        _state.VisualStudioPath = _vsPathEdit.Text.Trim();
        _state.VisualStudioCodePath = _vsCodePathEdit.Text.Trim();
    }

    private void UpdateIdePathVisibility()
    {
        var showVs = SelectedIde == DirectorIdeType.VisualStudio;
        _vsPathRow.Visibility = showVs;
        _vsCodePathRow.Visibility = !showVs;
    }

    private bool ValidateSettings()
    {
        if (string.IsNullOrWhiteSpace(_nameEdit.Text))
        {
            _windowManager.ShowNotification("Project name is required.", DirUINotificationType.Error);
            return false;
        }

        if (string.IsNullOrWhiteSpace(_folderEdit.Text))
        {
            _windowManager.ShowNotification("Project folder is required.", DirUINotificationType.Error);
            return false;
        }

        if (SelectedIde == DirectorIdeType.VisualStudio && string.IsNullOrWhiteSpace(_vsPathEdit.Text))
        {
            _windowManager.ShowNotification("Visual Studio path is required.", DirUINotificationType.Error);
            return false;
        }

        if (SelectedIde == DirectorIdeType.VisualStudioCode && string.IsNullOrWhiteSpace(_vsCodePathEdit.Text))
        {
            _windowManager.ShowNotification("VS Code path is required.", DirUINotificationType.Error);
            return false;
        }

        return true;
    }
}

