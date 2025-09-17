using System;
using System.Collections.Generic;
using BlingoEngine.Sounds;
using UnityEngine;

namespace BlingoEngine.Unity.Sounds;

/// <summary>
/// Unity implementation of <see cref="IBlingoFrameworkSound"/>.
/// </summary>
public class UnitySound : IBlingoFrameworkSound
{
    private BlingoSound _blingoSound = null!;
    private readonly List<BlingoSoundDevice> _devices = new();

    public List<BlingoSoundDevice> SoundDeviceList => _devices;

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

    internal void Init(BlingoSound sound)
    {
        _blingoSound = sound;
    }

    public void Beep() => Console.Beep();

    /// <summary>
    /// Gets a specific sound channel from the engine.
    /// </summary>
    public BlingoSoundChannel Channel(int channelNumber) => _blingoSound.Channel(channelNumber);

    /// <summary>
    /// Updates the list of output devices. Unity exposes only a single
    /// output device, so this simply resets the list to the default one.
    /// </summary>
    public void UpdateDeviceList()
    {
        _devices.Clear();
        _devices.Add(new BlingoSoundDevice(0, "Default"));
    }
}

