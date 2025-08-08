using System.IO;
using System.Text.Json;
using LingoEngine.Projects;

namespace LingoEngine.Director.Core.Projects;

public class LingoProjectSettingsRepository
{
    public void Save(string filePath, LingoProjectSettings settings)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(settings, options);
        File.WriteAllText(filePath, json);
    }

    public LingoProjectSettings Load(string filePath)
    {
        if (!File.Exists(filePath))
            return new LingoProjectSettings();
        var json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<LingoProjectSettings>(json) ?? new LingoProjectSettings();
    }
}
