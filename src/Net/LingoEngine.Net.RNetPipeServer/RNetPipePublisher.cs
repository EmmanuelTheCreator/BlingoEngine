using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using LingoEngine.Net.RNetContracts;
using LingoEngine.Net.RNetHost.Common;

namespace LingoEngine.Net.RNetPipeServer;

/// <summary>
/// Pipe-based implementation of <see cref="IRNetPublisher"/> that translates engine activity into
/// command messages delivered over the local pipe transport.
/// </summary>
public sealed class RNetPipePublisher : RNetPublisherBase
{
    private readonly IRNetPipeServer _pipeServer;
    private readonly Channel<SendMessage> _outgoing;
    private readonly CancellationTokenSource _sendCts = new();
    private readonly Task _sendLoop;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ConcurrentQueue<IRNetCommand> _commandQueue = new();
    private readonly Dictionary<string, Type> _commandTypes;
    private readonly Action<string, string> _messageHandler;

    private readonly record struct SendMessage(string Command, string Json);

    /// <summary>Initializes a new instance of the <see cref="RNetPipePublisher"/> class.</summary>
    public RNetPipePublisher(IRNetPipeServer pipeServer)
    {
        _pipeServer = pipeServer;
        _outgoing = Channel.CreateUnbounded<SendMessage>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
        _commandTypes = new Dictionary<string, Type>(StringComparer.Ordinal)
        {
            [nameof(SetSpritePropCmd)] = typeof(SetSpritePropCmd),
            [nameof(SetMemberPropCmd)] = typeof(SetMemberPropCmd),
            [nameof(GoToFrameCmd)] = typeof(GoToFrameCmd),
            [nameof(PauseCmd)] = typeof(PauseCmd),
            [nameof(ResumeCmd)] = typeof(ResumeCmd)
        };
        _messageHandler = OnClientMessage;
        _pipeServer.ClientMessageReceived += _messageHandler;
        _sendLoop = Task.Run(() => SendLoopAsync(_sendCts.Token));
    }

    /// <inheritdoc />
    public override bool TryDrainCommands(Action<IRNetCommand> apply)
    {
        var had = false;
        while (_commandQueue.TryDequeue(out var cmd))
        {
            had = true;
            apply(cmd);
        }

        return had;
    }

    /// <inheritdoc />
    protected override bool TryPublishFrameCore(StageFrameDto frame)
        => QueueMessage("StageFrame", frame);

    /// <inheritdoc />
    protected override bool TryPublishDeltaCore(SpriteDeltaDto delta)
        => QueueMessage("SpriteDelta", delta);

    /// <inheritdoc />
    protected override bool TryPublishKeyframeCore(KeyframeDto keyframe)
        => QueueMessage("Keyframe", keyframe);

    /// <inheritdoc />
    protected override bool TryPublishFilmLoopCore(FilmLoopDto filmLoop)
        => QueueMessage("FilmLoop", filmLoop);

    /// <inheritdoc />
    protected override bool TryPublishSoundCore(SoundEventDto sound)
        => QueueMessage("SoundEvent", sound);

    /// <inheritdoc />
    protected override bool TryPublishTempoCore(TempoDto tempo)
        => QueueMessage("Tempo", tempo);

    /// <inheritdoc />
    protected override bool TryPublishColorPaletteCore(ColorPaletteDto palette)
        => QueueMessage("ColorPalette", palette);

    /// <inheritdoc />
    protected override bool TryPublishFrameScriptCore(FrameScriptDto script)
        => QueueMessage("FrameScript", script);

    /// <inheritdoc />
    protected override bool TryPublishTransitionCore(TransitionDto transition)
        => QueueMessage("Transition", transition);

    /// <inheritdoc />
    protected override bool TryPublishTextStyleCore(TextStyleDto style)
        => QueueMessage("TextStyle", style);

    /// <inheritdoc />
    protected override bool TryPublishSpriteCollectionEventCore(RNetSpriteCollectionEventDto evt)
        => QueueMessage("SpriteCollectionEvent", evt);

    /// <inheritdoc />
    protected override bool TryPublishMemberPropertyCore(RNetMemberPropertyDto property)
        => QueueMessage("MemberProperty", property);

    /// <inheritdoc />
    protected override bool TryPublishMoviePropertyCore(RNetMoviePropertyDto property)
        => QueueMessage("MovieProperty", property);

    /// <inheritdoc />
    protected override bool TryPublishStagePropertyCore(RNetStagePropertyDto property)
        => QueueMessage("StageProperty", property);

    /// <summary>Attempts to enqueue a message for delivery to the pipe client.</summary>
    private bool QueueMessage<T>(string command, T payload)
    {
        string json;
        try
        {
            json = JsonSerializer.Serialize(payload, _jsonOptions);
        }
        catch
        {
            return false;
        }

        return _outgoing.Writer.TryWrite(new SendMessage(command, json));
    }

    private async Task SendLoopAsync(CancellationToken ct)
    {
        try
        {
            await foreach (var msg in _outgoing.Reader.ReadAllAsync(ct))
            {
                try
                {
                    await _pipeServer.SendRawAsync(msg.Command, msg.Json, ct).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    break;
                }
                catch
                {
                    // Ignore individual send failures to keep the loop running.
                }
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // Expected during shutdown.
        }
    }

    private void OnClientMessage(string command, string json)
    {
        if (!_commandTypes.TryGetValue(command, out var type))
        {
            return;
        }

        try
        {
            if (JsonSerializer.Deserialize(json, type, _jsonOptions) is IRNetCommand cmd)
            {
                _commandQueue.Enqueue(cmd);
            }
        }
        catch
        {
            // Ignore malformed payloads.
        }
    }

    /// <inheritdoc />
    public override void Disable()
    {
        base.Disable();
        _commandQueue.Clear();
    }
}
