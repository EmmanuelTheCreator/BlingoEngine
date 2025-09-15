using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using AbstUI;
using LingoEngine.Core;
using LingoEngine.Members;
using LingoEngine.Movies;
using LingoEngine.Sprites;
using LingoEngine.Stages;
using LingoEngine.Net.RNetContracts;

namespace LingoEngine.Net.RNetHost;

/// <summary>
/// Helpers used by the Lingo engine to publish remote debugging information.
/// </summary>
public interface IRNetPublisher
{
    /// <summary>Attempts to publish a stage frame without blocking.</summary>
    void TryPublishFrame(StageFrameDto frame);

    /// <summary>Attempts to publish a sprite delta without blocking.</summary>
    void TryPublishDelta(SpriteDeltaDto delta);

    /// <summary>Attempts to publish a keyframe without blocking.</summary>
    void TryPublishKeyframe(KeyframeDto keyframe);

    /// <summary>Attempts to publish a film loop state change without blocking.</summary>
    void TryPublishFilmLoop(FilmLoopDto filmLoop);

    /// <summary>Attempts to publish a sound event without blocking.</summary>
    void TryPublishSound(SoundEventDto sound);

    /// <summary>Attempts to publish a tempo change without blocking.</summary>
    void TryPublishTempo(TempoDto tempo);

    /// <summary>Attempts to publish a color palette without blocking.</summary>
    void TryPublishColorPalette(ColorPaletteDto palette);

    /// <summary>Attempts to publish a frame script without blocking.</summary>
    void TryPublishFrameScript(FrameScriptDto script);

    /// <summary>Attempts to publish a transition without blocking.</summary>
    void TryPublishTransition(TransitionDto transition);

    /// <summary>Attempts to publish a member property without blocking.</summary>
    void TryPublishMemberProperty(MemberPropertyDto property);

    /// <summary>Attempts to publish a movie property without blocking.</summary>
    void TryPublishMovieProperty(MoviePropertyDto property);

    /// <summary>Attempts to publish a stage property without blocking.</summary>
    void TryPublishStageProperty(StagePropertyDto property);

    /// <summary>Attempts to publish a text style without blocking.</summary>
    void TryPublishTextStyle(TextStyleDto style);

    /// <summary>Attempts to publish a sprite collection change without blocking.</summary>
    void TryPublishSpriteCollectionEvent(SpriteCollectionEventDto evt);

    /// <summary>Flushes queued property changes to the bus.</summary>
    void FlushQueuedProperties();

    /// <summary>Enables publishing by subscribing to the provided player.</summary>
    void Enable(ILingoPlayer player);

    /// <summary>Disables publishing and unsubscribes from all events.</summary>
    void Disable();

    /// <summary>Drains queued commands and applies them through the provided delegate.</summary>
    /// <param name="apply">Delegate invoked for each command.</param>
    /// <returns><c>true</c> if any command was processed; otherwise, <c>false</c>.</returns>
    bool TryDrainCommands(Action<INetCommand> apply);
}

/// <summary>
/// Default implementation of <see cref="IRNetPublisher"/>.
/// </summary>
internal sealed class RNetPublisher : IRNetPublisher
{
    private readonly IBus _bus;
    private readonly ConcurrentDictionary<int, SpriteDeltaDto> _spriteQueue = new();
    private readonly ConcurrentDictionary<(int CastLibNum, int NumberInCast, string Prop), MemberPropertyDto> _memberQueue = new();
    private readonly ConcurrentDictionary<string, MoviePropertyDto> _movieQueue = new();
    private readonly ConcurrentDictionary<string, StagePropertyDto> _stageQueue = new();
    private readonly Dictionary<IHasPropertyChanged, PropertyChangedEventHandler> _propSubs = new();
    private readonly List<Action> _managerUnsubs = new();

    private ILingoPlayer? _player;
    private ILingoMovie? _movie;

    /// <summary>Initializes a new instance of the <see cref="RNetPublisher"/> class.</summary>
    public RNetPublisher(IBus bus) => _bus = bus;

    /// <inheritdoc />
    public void TryPublishFrame(StageFrameDto frame)
        => _bus.Frames.Writer.TryWrite(frame);

    /// <inheritdoc />
    public void TryPublishDelta(SpriteDeltaDto delta)
        => _spriteQueue[delta.SpriteNum] = delta;

    /// <inheritdoc />
    public void TryPublishKeyframe(KeyframeDto keyframe)
        => _bus.Keyframes.Writer.TryWrite(keyframe);

    /// <inheritdoc />
    public void TryPublishFilmLoop(FilmLoopDto filmLoop)
        => _bus.FilmLoops.Writer.TryWrite(filmLoop);

    /// <inheritdoc />
    public void TryPublishSound(SoundEventDto sound)
        => _bus.Sounds.Writer.TryWrite(sound);

    /// <inheritdoc />
    public void TryPublishTempo(TempoDto tempo)
        => _bus.Tempos.Writer.TryWrite(tempo);

    /// <inheritdoc />
    public void TryPublishColorPalette(ColorPaletteDto palette)
        => _bus.ColorPalettes.Writer.TryWrite(palette);

    /// <inheritdoc />
    public void TryPublishFrameScript(FrameScriptDto script)
        => _bus.FrameScripts.Writer.TryWrite(script);

    /// <inheritdoc />
    public void TryPublishTransition(TransitionDto transition)
        => _bus.Transitions.Writer.TryWrite(transition);

    /// <inheritdoc />
    public void TryPublishMemberProperty(MemberPropertyDto property)
        => _memberQueue[(property.CastLibNum, property.NumberInCast, property.Prop)] = property;

    /// <inheritdoc />
    public void TryPublishMovieProperty(MoviePropertyDto property)
        => _movieQueue[property.Prop] = property;

    /// <inheritdoc />
    public void TryPublishStageProperty(StagePropertyDto property)
        => _stageQueue[property.Prop] = property;

    /// <inheritdoc />
    public void TryPublishTextStyle(TextStyleDto style)
        => _bus.TextStyles.Writer.TryWrite(style);

    /// <inheritdoc />
    public void TryPublishSpriteCollectionEvent(SpriteCollectionEventDto evt)
        => _bus.SpriteCollectionEvents.Writer.TryWrite(evt);

    /// <inheritdoc />
    public void FlushQueuedProperties()
    {
        foreach (var kv in _spriteQueue)
        {
            if (_bus.Deltas.Writer.TryWrite(kv.Value))
            {
                _spriteQueue.TryRemove(kv.Key, out _);
            }
        }

        foreach (var kv in _memberQueue)
        {
            if (_bus.MemberProperties.Writer.TryWrite(kv.Value))
            {
                _memberQueue.TryRemove(kv.Key, out _);
            }
        }

        foreach (var kv in _movieQueue)
        {
            if (_bus.MovieProperties.Writer.TryWrite(kv.Value))
            {
                _movieQueue.TryRemove(kv.Key, out _);
            }
        }

        foreach (var kv in _stageQueue)
        {
            if (_bus.StageProperties.Writer.TryWrite(kv.Value))
            {
                _stageQueue.TryRemove(kv.Key, out _);
            }
        }
    }

    /// <inheritdoc />
    public bool TryDrainCommands(Action<INetCommand> apply)
    {
        var reader = _bus.Commands.Reader;
        var had = false;
        while (reader.TryRead(out var cmd))
        {
            had = true;
            apply(cmd);
        }

        return had;
    }

    /// <inheritdoc />
    public void Enable(ILingoPlayer player)
    {
        if (_player is not null)
        {
            Disable();
        }

        _player = player;

        if (player.Stage is IHasPropertyChanged stage)
        {
            Subscribe(stage, OnStagePropertyChanged);
        }
        player.ActiveMovieChanged += OnActiveMovieChanged;

        foreach (var cast in player.CastLibs.GetAll())
        {
            cast.MemberAdded += OnMemberAdded;
            cast.MemberDeleted += OnMemberDeleted;
            foreach (var member in cast.GetAll())
            {
                SubscribeMember(member);
            }
        }

        if (player.ActiveMovie is ILingoMovie movie)
        {
            HookMovie(movie);
        }
    }

    /// <inheritdoc />
    public void Disable()
    {
        if (_player is null)
        {
            return;
        }

        foreach (var kv in _propSubs.ToArray())
        {
            kv.Key.PropertyChanged -= kv.Value;
            _propSubs.Remove(kv.Key);
        }

        _player.ActiveMovieChanged -= OnActiveMovieChanged;

        foreach (var cast in _player.CastLibs.GetAll())
        {
            cast.MemberAdded -= OnMemberAdded;
            cast.MemberDeleted -= OnMemberDeleted;
        }

        UnhookMovie();

        _player = null;
        _movie = null;
    }

    private void OnActiveMovieChanged(ILingoMovie? movie)
    {
        UnhookMovie();
        if (movie is not null)
        {
            HookMovie(movie);
        }
    }

    private void HookMovie(ILingoMovie movie)
    {
        _movie = movie;
        Subscribe(movie, OnMoviePropertyChanged);
        if (movie is LingoMovie lm)
        {
            lm.CurrentFrameChanged += OnFrameChanged;
            HookManager(lm.Sprite2DManager, "Sprite2D");
            HookManager(lm.Transitions, "Transition");
            HookManager(lm.Tempos, "Tempo");
            HookManager(lm.ColorPalettes, "ColorPalette");
            HookManager(lm.FrameScripts, "FrameScript");
            HookManager(lm.Audio, "Audio");
        }
    }

    private void UnhookMovie()
    {
        if (_movie is not null)
        {
            if (_movie is LingoMovie lm)
            {
                lm.CurrentFrameChanged -= OnFrameChanged;
            }
            Unsubscribe(_movie);
        }

        foreach (var unsub in _managerUnsubs)
        {
            unsub();
        }
        _managerUnsubs.Clear();
    }

    private void HookManager<TSprite>(ILingoSpriteManager<TSprite> manager, string name) where TSprite : LingoSprite
    {
        void Added(TSprite sprite)
        {
            SubscribeSprite(sprite);
            SpriteDto? dto = sprite is LingoSprite2D s2d ? ToSpriteDto(s2d) : null;
            TryPublishSpriteCollectionEvent(new SpriteCollectionEventDto(name, SpriteCollectionEventType.Added, sprite.SpriteNum, dto));
        }

        void Removed(TSprite sprite)
        {
            Unsubscribe(sprite);
            TryPublishSpriteCollectionEvent(new SpriteCollectionEventDto(name, SpriteCollectionEventType.Removed, sprite.SpriteNum, null));
        }

        void Cleared()
            => TryPublishSpriteCollectionEvent(new SpriteCollectionEventDto(name, SpriteCollectionEventType.Cleared, 0, null));

        manager.SpriteAdded += Added;
        manager.SpriteRemoved += Removed;
        manager.SpritesCleared += Cleared;

        foreach (var s in manager.GetAllSprites())
        {
            SubscribeSprite(s);
        }

        _managerUnsubs.Add(() =>
        {
            manager.SpriteAdded -= Added;
            manager.SpriteRemoved -= Removed;
            manager.SpritesCleared -= Cleared;
            foreach (var s in manager.GetAllSprites())
            {
                Unsubscribe(s);
            }
        });
    }

    private void OnFrameChanged(int _)
        => FlushQueuedProperties();

    private void OnMemberAdded(ILingoMember member)
        => SubscribeMember(member);

    private void OnMemberDeleted(ILingoMember member)
        => Unsubscribe(member);

    private void OnMemberPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is ILingoMember member && e.PropertyName is not null)
        {
            var value = member.GetType().GetProperty(e.PropertyName, BindingFlags.Public | BindingFlags.Instance)?.GetValue(member)?.ToString() ?? string.Empty;
            TryPublishMemberProperty(new MemberPropertyDto(member.CastLibNum, member.NumberInCast, member.Name, e.PropertyName, value));
        }
    }

    private void OnMoviePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is ILingoMovie movie && e.PropertyName is not null)
        {
            var value = movie.GetType().GetProperty(e.PropertyName, BindingFlags.Public | BindingFlags.Instance)?.GetValue(movie)?.ToString() ?? string.Empty;
            TryPublishMovieProperty(new MoviePropertyDto(e.PropertyName, value));
        }
    }

    private void OnStagePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is ILingoStage stage && e.PropertyName is not null)
        {
            var value = stage.GetType().GetProperty(e.PropertyName, BindingFlags.Public | BindingFlags.Instance)?.GetValue(stage)?.ToString() ?? string.Empty;
            TryPublishStageProperty(new StagePropertyDto(e.PropertyName, value));
        }
    }

    private void SubscribeMember(ILingoMember member)
    {
        PropertyChangedEventHandler handler = OnMemberPropertyChanged;
        Subscribe(member, handler);
    }

    private void SubscribeSprite(LingoSprite sprite)
    {
        PropertyChangedEventHandler handler = (_, __) => { };
        // Currently no per-property sprite publishing.
        Subscribe(sprite, handler);
    }

    private void Subscribe(IHasPropertyChanged src, PropertyChangedEventHandler handler)
    {
        src.PropertyChanged += handler;
        _propSubs[src] = handler;
    }

    private void Unsubscribe(IHasPropertyChanged src)
    {
        if (_propSubs.Remove(src, out var h))
        {
            src.PropertyChanged -= h;
        }
    }

    private static SpriteDto ToSpriteDto(LingoSprite2D sprite)
    {
        var memberId = sprite.Member is ILingoMember m ? (m.CastLibNum * 131114) + m.NumberInCast : 0;
        return new SpriteDto(sprite.SpriteNum, sprite.LocZ, memberId, (int)sprite.LocH, (int)sprite.LocV, (int)sprite.Width, (int)sprite.Height, (int)sprite.Rotation, (int)sprite.Skew, (int)sprite.Blend, sprite.Ink);
    }
}
