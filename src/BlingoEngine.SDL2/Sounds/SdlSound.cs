using System;
using System.Runtime.InteropServices;
using BlingoEngine.Sounds;
using AbstUI.SDL2.SDLL;

namespace BlingoEngine.SDL2.Sounds;

public class SdlSound : IBlingoFrameworkSound
{
    private BlingoSound _blingoSound = null!;
    private readonly List<BlingoSoundDevice> _devices = new();
    private bool _initialized;

    public List<BlingoSoundDevice> SoundDeviceList => _devices;
    public int SoundLevel { get; set; }
    public bool SoundEnable { get; set; } = true;

    internal void Init(BlingoSound sound)
    {
        _blingoSound = sound;
        if (!_initialized)
        {
            SDL_mixer.Mix_OpenAudio(44100, SDL.AUDIO_S16LSB, 2, 2048);
            _initialized = true;
        }
    }

    public void Beep() => Console.Beep();
    public BlingoSoundChannel Channel(int number) => _blingoSound.Channel(number);
    public void UpdateDeviceList()
    {
        _devices.Clear();
        int count = SDL.SDL_GetNumAudioDevices(0);
        for (int i = 0; i < count; i++)
        {
            var name = SDL.SDL_GetAudioDeviceName(i, 0);
            _devices.Add(new BlingoSoundDevice(i, name ?? string.Empty));
        }
    }
}

