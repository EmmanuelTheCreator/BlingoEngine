namespace LingoEngine.Director.Core.Projects;

using AbstUI.Primitives;

public enum DirectorIdeType
{
    VisualStudio,
    VisualStudioCode
}
public class DirectorProjectSettings
{
    public string? VisualStudioPath { get; set; } = @"C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\devenv.exe";
    public string? VisualStudioCodePath { get; set; } = @"C:\Program Files\Microsoft VS Code\Code.exe";
    public DirectorIdeType PreferredIde { get; set; } = DirectorIdeType.VisualStudio;
    public string CsProjFile { get; set; } = string.Empty;

    // Stage guides
    public AColor GuidesColor { get; set; } = AColors.Blue;
    public bool GuidesVisible { get; set; } = true;
    public bool GuidesSnap { get; set; }
    public bool GuidesLocked { get; set; }
    public List<float> VerticalGuides { get; set; } = new();
    public List<float> HorizontalGuides { get; set; } = new();

    // Stage grid
    public AColor GridColor { get; set; } = AColors.Gray;
    public bool GridVisible { get; set; }
    public bool GridSnap { get; set; }
    public float GridWidth { get; set; } = 32;
    public float GridHeight { get; set; } = 32;

    // Window positions
    public Dictionary<string, DirectorWindowState> WindowStates { get; set; } = new();
}
