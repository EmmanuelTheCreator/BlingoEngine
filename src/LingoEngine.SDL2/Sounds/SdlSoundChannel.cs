using AbstUI.SDL2.SDLL;
using LingoEngine.Sounds;
using System.Collections.Generic;
using System.Diagnostics;

namespace LingoEngine.SDL2.Sounds;

public class SdlSoundChannel : ILingoFrameworkSoundChannel, IDisposable
{
    private static readonly Dictionary<int, SdlSoundChannel> _channels = new();
    private static readonly SDL_mixer.ChannelFinishedDelegate _channelFinishedCallback = OnChannelFinished;
    private bool _desiredStateStop = false;
    static SdlSoundChannel()
    {
        SDL_mixer.Mix_ChannelFinished(_channelFinishedCallback);
    }

    private readonly Stopwatch _elapsedTimer = new();
    private LingoSoundChannel _lingoSoundChannel = null!;
    private nint _chunk = nint.Zero;
    private string? _currentFile;
    private int _volume;

    public int ChannelNumber { get; }
    public int SampleRate => 44100;
    public int Volume
    {
        get => _volume;
        set
        {
            _volume = value;
            var newValue = (Volume / 100f) * SDL_mixer.MIX_MAX_VOLUME;
            SDL_mixer.Mix_Volume(ChannelNumber, (int)newValue);
        }
    }
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
        _channels[ChannelNumber] = this;
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
        _desiredStateStop = false;
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
        _desiredStateStop = false;
    }

    public void Stop()
    {
        _desiredStateStop = true;
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
        if (_desiredStateStop)
            return;
        if (_chunk == nint.Zero && _currentFile != null)
        {
            _chunk = SDL_mixer.Mix_LoadWAV(_currentFile);
        }
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
        if (_channels.TryGetValue(channel, out var ch))
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
