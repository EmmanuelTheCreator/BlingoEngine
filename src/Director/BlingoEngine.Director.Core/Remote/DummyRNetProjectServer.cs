using System;
using System.Threading;
using System.Threading.Tasks;
using BlingoEngine.Core;
using BlingoEngine.Net.RNetContracts;
using BlingoEngine.Net.RNetHost.Common;

namespace BlingoEngine.Director.Core.Remote;

/// <summary>
/// No-op implementation used when the SignalR project host cannot be loaded (e.g., in Godot).
/// </summary>
public sealed class DummyRNetProjectServer : IRNetProjectServer
{
    private readonly IRNetPublisherEngineBridge _publisher = new NoopPublisher();
    private BlingoNetConnectionState _state = BlingoNetConnectionState.Disconnected;

    /// <inheritdoc />
    public IRNetPublisherEngineBridge Publisher => _publisher;

    /// <inheritdoc />
    public BlingoNetConnectionState ConnectionState
    {
        get => _state;
        private set
        {
            if (_state == value)
            {
                return;
            }

            _state = value;
            ConnectionStatusChanged?.Invoke(_state);
        }
    }

    /// <inheritdoc />
    public bool IsEnabled => ConnectionState == BlingoNetConnectionState.Connected;

    /// <inheritdoc />
    public event Action<BlingoNetConnectionState>? ConnectionStatusChanged;

    /// <inheritdoc />
#pragma warning disable CS0067 // The event is part of the contract but never raised by the dummy implementation.
    public event Action<IRNetCommand>? NetCommandReceived;
#pragma warning restore CS0067

    /// <inheritdoc />
    public Task StartAsync(CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
        {
            return Task.FromCanceled(ct);
        }

        ConnectionState = BlingoNetConnectionState.Connected;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync()
    {
        ConnectionState = BlingoNetConnectionState.Disconnected;
        return Task.CompletedTask;
    }

    private sealed class NoopPublisher : IRNetPublisherEngineBridge
    {
        public void Enable(IBlingoPlayer player)
        {
        }

        public void Disable()
        {
        }

        public void TryPublishFrame(StageFrameDto frame)
        {
        }

        public void TryPublishDelta(SpriteDeltaDto delta)
        {
        }

        public void TryPublishKeyframe(KeyframeDto keyframe)
        {
        }

        public void TryPublishFilmLoop(FilmLoopDto filmLoop)
        {
        }

        public void TryPublishSound(SoundEventDto sound)
        {
        }

        public void TryPublishTempo(TempoDto tempo)
        {
        }

        public void TryPublishColorPalette(ColorPaletteDto palette)
        {
        }

        public void TryPublishFrameScript(FrameScriptDto script)
        {
        }

        public void TryPublishTransition(TransitionDto transition)
        {
        }

        public void TryPublishMemberProperty(RNetMemberPropertyDto property)
        {
        }

        public void TryPublishMovieProperty(RNetMoviePropertyDto property)
        {
        }

        public void TryPublishStageProperty(RNetStagePropertyDto property)
        {
        }

        public void TryPublishTextStyle(TextStyleDto style)
        {
        }

        public void TryPublishSpriteCollectionEvent(RNetSpriteCollectionEventDto evt)
        {
        }

        public void FlushQueuedProperties()
        {
        }

        public bool TryDrainCommands(Action<IRNetCommand> apply) => false;
    }
}

