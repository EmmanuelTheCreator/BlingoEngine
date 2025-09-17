namespace BlingoEngine.Projects;

using System.IO;


public class BlingoProjectSettings
{
    public string ProjectName { get; set; } = string.Empty;
    public string ProjectFolder { get; set; } = string.Empty;
    public string CodeFolder { get; set; } = string.Empty;
    public int StageWidth { get; set; }
    public int StageHeight { get; set; }
    public bool HasValidSettings =>
        !string.IsNullOrWhiteSpace(ProjectName) &&
        !string.IsNullOrWhiteSpace(ProjectFolder);

    public int MaxSpriteChannelCount { get; set; } = 20;



    public BlingoProjectSettings()
    {

    }

   
}

