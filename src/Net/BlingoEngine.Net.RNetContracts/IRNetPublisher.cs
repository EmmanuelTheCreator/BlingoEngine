using System;

namespace BlingoEngine.Net.RNetContracts;

/// <summary>
/// Helpers used by the BlingoEngine runtime to publish remote debugging information.
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
    void TryPublishMemberProperty(RNetMemberPropertyDto property);

    /// <summary>Attempts to publish a movie property without blocking.</summary>
    void TryPublishMovieProperty(RNetMoviePropertyDto property);

    /// <summary>Attempts to publish a stage property without blocking.</summary>
    void TryPublishStageProperty(RNetStagePropertyDto property);

    /// <summary>Attempts to publish a text style without blocking.</summary>
    void TryPublishTextStyle(TextStyleDto style);

    /// <summary>Attempts to publish a sprite collection change without blocking.</summary>
    void TryPublishSpriteCollectionEvent(RNetSpriteCollectionEventDto evt);

    /// <summary>Flushes queued property changes to the bus.</summary>
    void FlushQueuedProperties();

    /// <summary>Drains queued commands and applies them through the provided delegate.</summary>
    /// <param name="apply">Delegate invoked for each command.</param>
    /// <returns><c>true</c> if any command was processed; otherwise, <c>false</c>.</returns>
    bool TryDrainCommands(Action<IRNetCommand> apply);
}

