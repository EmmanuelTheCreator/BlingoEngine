using System.Collections.Generic;
using BlingoEngine.Sounds;
using AbstUI.Blazor;

namespace BlingoEngine.Blazor.Sounds;

/// <summary>
/// Minimal sound implementation for the Blazor backend.
/// Provides no real audio playback but keeps track of settings.
/// </summary>
public class BlingoBlazorSound : IBlingoFrameworkSound
{
    private readonly List<BlingoSoundDevice> _devices = new();
    private BlingoSound _sound = null!;
    private readonly AbstUIScriptResolver _scripts;

    public List<BlingoSoundDevice> SoundDeviceList => _devices;
    public int SoundLevel { get; set; }
    public bool SoundEnable { get; set; } = true;

    public BlingoBlazorSound(AbstUIScriptResolver scripts)
    {
        _scripts = scripts;
    }

    internal void Init(BlingoSound sound) => _sound = sound;

    public void Beep() => _scripts.MediaBeep().GetAwaiter().GetResult();
}

