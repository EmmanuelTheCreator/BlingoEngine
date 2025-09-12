using System.Collections.Generic;
using LingoEngine.Sounds;
using AbstUI.Blazor;

namespace LingoEngine.Blazor.Sounds;

/// <summary>
/// Minimal sound implementation for the Blazor backend.
/// Provides no real audio playback but keeps track of settings.
/// </summary>
public class LingoBlazorSound : ILingoFrameworkSound
{
    private readonly List<LingoSoundDevice> _devices = new();
    private LingoSound _sound = null!;
    private readonly AbstUIScriptResolver _scripts;

    public List<LingoSoundDevice> SoundDeviceList => _devices;
    public int SoundLevel { get; set; }
    public bool SoundEnable { get; set; } = true;

    public LingoBlazorSound(AbstUIScriptResolver scripts)
    {
        _scripts = scripts;
    }

    internal void Init(LingoSound sound) => _sound = sound;

    public void Beep() => _scripts.MediaBeep().GetAwaiter().GetResult();
}
