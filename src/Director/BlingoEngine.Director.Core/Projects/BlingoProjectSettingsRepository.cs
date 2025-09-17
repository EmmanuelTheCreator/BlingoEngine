using System.IO;
using System.Text.Json;
using BlingoEngine.Projects;

namespace BlingoEngine.Director.Core.Projects;

public class BlingoProjectSettingsRepository
{
    public void Save(string filePath, BlingoProjectSettings settings)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(settings, options);
        File.WriteAllText(filePath, json);
    }

    public BlingoProjectSettings Load(string filePath)
    {
        if (!File.Exists(filePath))
            return new BlingoProjectSettings();
        var json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<BlingoProjectSettings>(json) ?? new BlingoProjectSettings();
    }
}

