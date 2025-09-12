using System;
using LingoEngine.Sounds;
using LingoEngine.Members;
using Microsoft.JSInterop;
using AbstUI.Blazor;
using LingoEngine.Blazor.Util;

namespace LingoEngine.Blazor.Sounds;

/// <summary>
/// Placeholder sound channel for the Blazor backend.
/// Implements the required interface without real audio output.
/// </summary>
public class LingoBlazorSoundChannel : ILingoFrameworkSoundChannel, IDisposable
{
    private LingoSoundChannel _channel = null!;
    private readonly AbstUIScriptResolver _scripts;
    private Lazy<IJSObjectReference> _audio;
    private DotNetObjectReference<object>? _audioRef;

    public LingoBlazorSoundChannel(int number, AbstUIScriptResolver scripts)
    {
        ChannelNumber = number;
        _scripts = scripts;
        _audioRef = DotNetObjectReference.Create<object>(this);
        var id = ElementIdGenerator.Create(number.ToString());
        _audio = new Lazy<IJSObjectReference>(() =>  _scripts.AudioCreate(id, _audioRef).GetAwaiter().GetResult());
    }

    public int ChannelNumber { get; }
    public int SampleRate => 44100;
    private int _volume;
    public int Volume { get => _volume; 
        set 
        { 
            if (_volume == value) return;
            _volume = value;
            _scripts.AudioSetVolume(_audio.Value, value / 255.0).GetAwaiter().GetResult(); } }
    public int Pan { get; set; }
    private float _currentTime;
    public float CurrentTime
    {
        get => _currentTime;
        set
        {
            if (_currentTime == value) return;
            _scripts.AudioSeek(_audio.Value, value).GetAwaiter().GetResult();
            _currentTime = value;
        }
    }
    public bool IsPlaying { get; private set; }
    public int ChannelCount => 2;
    public int SampleCount => 0;
    public float ElapsedTime => _currentTime;
    public float StartTime { get; set; }
    public float EndTime { get; set; }

    internal void Init(LingoSoundChannel channel) => _channel = channel;

    public void Pause()
    {
        if (!IsPlaying)
        {
            _scripts.AudioResume(_audio.Value).GetAwaiter().GetResult();
            IsPlaying = true;
        }
        else
        {
            _scripts.AudioPause(_audio.Value).GetAwaiter().GetResult();
            IsPlaying = false;
        }
    }
    public void PlayFile(string stringFilePath)
    {
        _scripts.AudioPlay(_audio.Value, stringFilePath).GetAwaiter().GetResult();
        IsPlaying = true;
    }
    public void PlayNow(LingoMemberSound member)
    {
        var url = member.Framework<LingoBlazorMemberSound>().Url;
        if (url != null)
        {
            _scripts.AudioPlay(_audio.Value, url).GetAwaiter().GetResult();
            IsPlaying = true;
        }
    }
    public void Repeat()
    {
        _scripts.AudioSeek(_audio.Value, StartTime).GetAwaiter().GetResult() ;
        _scripts.AudioResume(_audio.Value).GetAwaiter().GetResult();
        IsPlaying = true;
    }
    public void Rewind()
    {
        _scripts.AudioSeek(_audio.Value, 0).GetAwaiter().GetResult();
        _currentTime = 0;
    }
    public void Stop()
    {
        _scripts.AudioStop(_audio.Value).GetAwaiter().GetResult();
        IsPlaying = false;
        _currentTime = 0;
    }

    [JSInvokable]
    public void OnSoundEnded()
    {
        IsPlaying = false;
        _currentTime = 0;
        _channel.SoundFinished();
    }

    public void Dispose()
    {
        _audio.Value.DisposeAsync().GetAwaiter().GetResult();
        _audioRef?.Dispose();
    }
}
