using AbstUI.SDL2.SDLL;
using LingoEngine.Sounds;
using System.Collections.Generic;
using System.Diagnostics;

namespace LingoEngine.SDL2.Sounds;

public class SdlSoundChannel : ILingoFrameworkSoundChannel, IDisposable
{
    private static readonly Dictionary<int, SdlSoundChannel> Channels = new();
    private static readonly SDL_mixer.ChannelFinishedDelegate ChannelFinishedCallback = OnChannelFinished;

    static SdlSoundChannel()
    {
        SDL_mixer.Mix_ChannelFinished(ChannelFinishedCallback);
    }

    private readonly Stopwatch _elapsedTimer = new();
    private LingoSoundChannel _lingoSoundChannel = null!;
    private nint _chunk = nint.Zero;
    private string? _currentFile;
    public int ChannelNumber { get; }
    public int SampleRate => 44100;
    public int Volume { get; set; }
    public int Pan { get; set; }
    public float CurrentTime { get; set; }
    public bool IsPlaying { get; private set; }
    public int ChannelCount => 2;
    public int SampleCount => 0;
    public float ElapsedTime => (float)_elapsedTimer.Elapsed.TotalSeconds;
    public float StartTime { get; set; }
    public float EndTime { get; set; }

    public SdlSoundChannel(int number)
    {
        ChannelNumber = number;
    }
    internal void Init(LingoSoundChannel channel)
    {
        _lingoSoundChannel = channel;
        Channels[ChannelNumber] = this;
    }

    public void PlayFile(string stringFilePath)
    {
        Stop();
        _currentFile = stringFilePath;
        _chunk = SDL_mixer.Mix_LoadWAV(stringFilePath);
        if (_chunk == nint.Zero)
            return;
        SDL_mixer.Mix_PlayChannel(ChannelNumber, _chunk, 0);
        IsPlaying = true;
        CurrentTime = StartTime;
        _elapsedTimer.Restart();
    }

    public void PlayNow(LingoMemberSound member)
    {
        Stop();
        _currentFile = member.FileName;
        _chunk = SDL_mixer.Mix_LoadWAV(_currentFile);
        if (_chunk == nint.Zero)
            return;
        SDL_mixer.Mix_PlayChannel(ChannelNumber, _chunk, 0);
        IsPlaying = true;
        CurrentTime = StartTime;
        _elapsedTimer.Restart();
    }

    public void Stop()
    {
        SDL_mixer.Mix_HaltChannel(ChannelNumber);
        if (_chunk != nint.Zero)
        {
            SDL_mixer.Mix_FreeChunk(_chunk);
            _chunk = nint.Zero;
        }
        IsPlaying = false;
        CurrentTime = 0;
        _elapsedTimer.Reset();
    }

    public void Rewind()
    {
        SDL_mixer.Mix_HaltChannel(ChannelNumber);
        CurrentTime = 0;
        _elapsedTimer.Reset();
    }

    public void Pause()
    {
        if (IsPlaying)
        {
            SDL_mixer.Mix_Pause(ChannelNumber);
            _elapsedTimer.Stop();
        }
        else
        {
            SDL_mixer.Mix_Resume(ChannelNumber);
            _elapsedTimer.Start();
        }
        IsPlaying = !IsPlaying;
    }

    public void Repeat()
    {
        if (_chunk != nint.Zero)
        {
            SDL_mixer.Mix_HaltChannel(ChannelNumber);
            SDL_mixer.Mix_PlayChannel(ChannelNumber, _chunk, -1);
            IsPlaying = true;
            _elapsedTimer.Restart();
        }
    }
    private static void OnChannelFinished(int channel)
    {
        if (Channels.TryGetValue(channel, out var ch))
        {
            ch.IsPlaying = false;
            if (ch._chunk != nint.Zero)
            {
                SDL_mixer.Mix_FreeChunk(ch._chunk);
                ch._chunk = nint.Zero;
            }
            ch._elapsedTimer.Reset();
            ch.CurrentTime = 0;
            ch.Sound_Finished();
        }
    }
    private void Sound_Finished()
    {
        try
        {
            _lingoSoundChannel.SoundFinished();
        }
        catch
        {
        }
    }
    public void Dispose()
    {
        Stop();
    }
}
