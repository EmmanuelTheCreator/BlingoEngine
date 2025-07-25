namespace LingoEngine.Director.Core.Projects;
using LingoEngine.Primitives;

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

    // Stage guides
    public LingoColor GuidesColor { get; set; } = LingoColorList.Blue;
    public bool GuidesVisible { get; set; } = true;
    public bool GuidesSnap { get; set; }
    public bool GuidesLocked { get; set; }
    public List<float> VerticalGuides { get; set; } = new();
    public List<float> HorizontalGuides { get; set; } = new();

    // Stage grid
    public LingoColor GridColor { get; set; } = LingoColorList.Gray;
    public bool GridVisible { get; set; }
    public bool GridSnap { get; set; }
    public float GridWidth { get; set; } = 32;
    public float GridHeight { get; set; } = 32;

    // Window positions
    public Dictionary<string, DirectorWindowState> WindowStates { get; set; } = new();
}
