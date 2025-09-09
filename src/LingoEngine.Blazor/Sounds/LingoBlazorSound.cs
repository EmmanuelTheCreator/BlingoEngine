using System.Collections.Generic;
using LingoEngine.Sounds;

namespace LingoEngine.Blazor.Sounds;

/// <summary>
/// Minimal sound implementation for the Blazor backend.
/// Provides no real audio playback but keeps track of settings.
/// </summary>
public class LingoBlazorSound : ILingoFrameworkSound
{
    private readonly List<LingoSoundDevice> _devices = new();
    private LingoSound _sound = null!;

    public List<LingoSoundDevice> SoundDeviceList => _devices;
    public int SoundLevel { get; set; }
    public bool SoundEnable { get; set; } = true;

    internal void Init(LingoSound sound) => _sound = sound;

    public void Beep() { }
}
