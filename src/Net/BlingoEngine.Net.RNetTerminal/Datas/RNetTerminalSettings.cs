using System.IO;
using System.Text.Json;

namespace BlingoEngine.Net.RNetTerminal.Datas;

/// <summary>
/// Settings persisted for the RNet terminal.
/// </summary>
public sealed class RNetTerminalSettings
{
    /// <summary>Port to connect to when using a remote host.</summary>
    public int Port { get; set; } = 61699;

    /// <summary>Preferred transport for future connections.</summary>
    public RNetTerminalTransport PreferredTransport { get; set; } = RNetTerminalTransport.Http;

    private static string GetPath() => Path.Combine(System.AppContext.BaseDirectory, "RNetTerminal.settings");

    /// <summary>Load settings from disk or return defaults.</summary>
    public static RNetTerminalSettings Load()
    {
        var path = GetPath();
        if (File.Exists(path))
        {
            try
            {
                var json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<RNetTerminalSettings>(json) ?? new RNetTerminalSettings();
            }
            catch
            {
                // ignore malformed files
            }
        }
        return new RNetTerminalSettings();
    }

    /// <summary>Persist the settings to disk.</summary>
    public void Save()
    {
        var path = GetPath();
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
    }
}


