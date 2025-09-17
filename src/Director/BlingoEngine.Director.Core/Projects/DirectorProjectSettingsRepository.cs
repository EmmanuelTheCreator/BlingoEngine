using System.Text.Json;

namespace BlingoEngine.Director.Core.Projects;

public class DirectorProjectSettingsRepository
{
    public void Save(string filePath, DirectorProjectSettings settings)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(settings, options);
        File.WriteAllText(filePath, json);
    }

    public DirectorProjectSettings Load(string filePath)
    {
        if (!File.Exists(filePath))
            return new DirectorProjectSettings();
        var json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<DirectorProjectSettings>(json) ?? new DirectorProjectSettings();
    }
}

