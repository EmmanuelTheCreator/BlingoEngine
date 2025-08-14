using System.Collections.Generic;
using LingoEngine.Sounds;

namespace LingoEngine.Unity.Sounds;

/// <summary>
/// Minimal sound system stub for the Unity backend.
/// </summary>
public class UnitySound : ILingoFrameworkSound
{
    private LingoSound _lingoSound = null!;

    public List<LingoSoundDevice> SoundDeviceList { get; } = new();

    public int SoundLevel { get; set; }

    public bool SoundEnable { get; set; } = true;

    internal void Init(LingoSound sound)
    {
        _lingoSound = sound;
    }

    public void Beep() { }
}
