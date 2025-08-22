using System;
using System.Collections.Generic;
using LingoEngine.Sounds;
using UnityEngine;

namespace LingoEngine.Unity.Sounds;

/// <summary>
/// Unity implementation of <see cref="ILingoFrameworkSound"/>.
/// </summary>
public class UnitySound : ILingoFrameworkSound
{
    private LingoSound _lingoSound = null!;
    private readonly List<LingoSoundDevice> _devices = new();

    public List<LingoSoundDevice> SoundDeviceList => _devices;

    public int SoundLevel
    {
        get => (int)(AudioListener.volume * 100f);
        set => AudioListener.volume = Mathf.Clamp01(value / 100f);
    }

    public bool SoundEnable
    {
        get => !AudioListener.pause;
        set => AudioListener.pause = !value;
    }

    internal void Init(LingoSound sound)
    {
        _lingoSound = sound;
    }

    public void Beep() => Console.Beep();

    /// <summary>
    /// Gets a specific sound channel from the engine.
    /// </summary>
    public LingoSoundChannel Channel(int channelNumber) => _lingoSound.Channel(channelNumber);

    /// <summary>
    /// Updates the list of output devices. Unity exposes only a single
    /// output device, so this simply resets the list to the default one.
    /// </summary>
    public void UpdateDeviceList()
    {
        _devices.Clear();
        _devices.Add(new LingoSoundDevice(0, "Default"));
    }
}
