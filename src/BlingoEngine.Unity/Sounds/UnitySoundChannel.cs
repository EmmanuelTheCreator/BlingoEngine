using System;
using BlingoEngine.Sounds;
using UnityEngine;

namespace BlingoEngine.Unity.Sounds;

/// <summary>
/// Unity implementation of <see cref="IBlingoFrameworkSoundChannel"/>.
/// </summary>
public class UnitySoundChannel : IBlingoFrameworkSoundChannel, IDisposable
{
    private readonly AudioSource _audioSource;
    private readonly ChannelBehaviour _behaviour;
    private BlingoSoundChannel _blingoSoundChannel = null!;
    private bool _wasPlaying;
    private float _startTime;
    private float _endTime;

    public int ChannelNumber { get; }
    public int SampleRate => _audioSource.clip?.frequency ?? 44100;
    public int Volume
    {
        get => (int)(_audioSource.volume * 255f);
        set => _audioSource.volume = Mathf.Clamp01(value / 255f);
    }
    public int Pan
    {
        get => (int)(_audioSource.pan * 100f);
        set => _audioSource.pan = Mathf.Clamp(value / 100f, -1f, 1f);
    }
    public float CurrentTime
    {
        get => _audioSource.time;
        set => _audioSource.time = value;
    }
    public bool IsPlaying => _audioSource.isPlaying;
    public int ChannelCount => _audioSource.clip?.channels ?? 0;
    public int SampleCount => _audioSource.clip?.samples ?? 0;
    public float ElapsedTime => _audioSource.time;
    public float StartTime
    {
        get => _startTime;
        set => _startTime = value;
    }
    public float EndTime
    {
        get => _endTime;
        set => _endTime = value;
    }

    public UnitySoundChannel(int number)
    {
        ChannelNumber = number;
        var go = new GameObject($"SoundChannel {number}");
        _audioSource = go.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
        _behaviour = go.AddComponent<ChannelBehaviour>();
        _behaviour.Owner = this;
    }

    internal void Init(BlingoSoundChannel channel)
    {
        _blingoSoundChannel = channel;
    }

    public void Pause()
    {
        if (_audioSource.isPlaying)
            _audioSource.Pause();
        else
            _audioSource.Play();
    }

    public void PlayFile(string stringFilePath)
    {
        var clip = UnityMemberSound.LoadClip(stringFilePath, out _, out _, out _);
        if (clip == null)
            return;
        Play(clip);
    }

    public void PlayNow(BlingoMemberSound member)
    {
        var clip = member.Framework<UnityMemberSound>().Clip;
        if (clip == null)
            return;
        Play(clip);
    }

    public void Repeat()
    {
        Rewind();
        _audioSource.Play();
        _wasPlaying = true;
    }

    public void Rewind()
    {
        _audioSource.time = 0f;
    }

    public void Stop()
    {
        _audioSource.Stop();
        _wasPlaying = false;
    }

    private void Play(AudioClip clip)
    {
        _audioSource.clip = clip;
        _endTime = clip.length;
        _audioSource.time = _startTime;
        _audioSource.Play();
        _wasPlaying = true;
    }

    private void SoundFinished()
    {
        try
        {
            _blingoSoundChannel.SoundFinished();
        }
        catch
        {
        }
    }

    public void Dispose()
    {
        if (_audioSource != null)
            UnityEngine.Object.Destroy(_audioSource.gameObject);
    }

    private class ChannelBehaviour : MonoBehaviour
    {
        public UnitySoundChannel Owner = null!;
        private void Update()
        {
            var src = Owner._audioSource;
            if (Owner._wasPlaying && !src.isPlaying)
            {
                Owner._wasPlaying = false;
                Owner.SoundFinished();
            }
        }
    }
}

