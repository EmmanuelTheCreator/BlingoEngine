using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using LingoEngine.Core;
using LingoEngine.Events;
using LingoEngine.Inputs;
using LingoEngine.Movies;
using LingoEngine.Sprites;

namespace LingoEngine.Lingo.Tests.TestDoubles;

internal static class FakeLingoMovieBuilder
{
    internal static FakeLingoMovieHarness Create(LingoEventMediator mediator, List<string> timeline, Action<FakeLingoMovieOptions>? configure = null)
    {
        var options = new FakeLingoMovieOptions();
        configure?.Invoke(options);

        var movie = (LingoMovie)FormatterServices.GetUninitializedObject(typeof(LingoMovie));

        var spriteManagers = new List<LingoSpriteManager>();
        PrivateFieldSetter.SetField(movie, "_spriteManagers", spriteManagers);
        PrivateFieldSetter.SetField(movie, "_eventMediator", mediator);
        PrivateFieldSetter.SetField(movie, "_actorList", new ActorList());
        PrivateFieldSetter.SetField(movie, "_idleHandlerPeriod", 1);
        PrivateFieldSetter.SetField(movie, "_idleIntervalSeconds", 1f / 60f);
        PrivateFieldSetter.SetField(movie, "_currentFrame", 0);
        PrivateFieldSetter.SetField(movie, "_lastFrame", 0);
        PrivateFieldSetter.SetField(movie, "_nextFrame", -1);
        PrivateFieldSetter.SetField(movie, "_needToRaiseStartMovie", false);
        PrivateFieldSetter.SetField(movie, "_hasPendingEndSprites", false);
        PrivateFieldSetter.SetField(movie, "_frameIsActive", false);
        PrivateFieldSetter.SetField(movie, "_idleAccumulator", 0f);

        var mouse = (LingoStageMouse)FormatterServices.GetUninitializedObject(typeof(LingoStageMouse));
        PrivateFieldSetter.SetField(movie, "_lingoMouse", mouse);

        var frameworkMovie = new FakeFrameworkMovie();
        PrivateFieldSetter.SetField(movie, "_frameworkMovie", frameworkMovie);

        var sprite2DManager = FakeSprite2DManager.Create(movie, mouse, timeline);
        PrivateFieldSetter.SetField(movie, "_sprite2DManager", sprite2DManager);

        var transitionManager = FakeTransitionManager.Create(movie, mediator, timeline);
        PrivateFieldSetter.SetField(movie, "_transitionManager", transitionManager);

        var transitionPlayer = options.TransitionPlayer ?? new FakeTransitionPlayer(
            options.TransitionStartLabel != null ? timeline : null,
            options.TransitionStartLabel);
        PrivateFieldSetter.SetField(movie, "_transitionPlayer", transitionPlayer);

        if (options.RecordTransitionLifecycle)
        {
            transitionManager.IsLifecycleRecordingEnabled = true;
            transitionManager.SetActivationFrame(options.TransitionActivationFrame);
            spriteManagers.Add(transitionManager);
        }
        else
        {
            transitionManager.IsLifecycleRecordingEnabled = false;
            transitionManager.SetActivationFrame(int.MaxValue);
        }

        return new FakeLingoMovieHarness(movie, sprite2DManager, transitionManager, transitionPlayer);
    }
}

internal sealed class FakeLingoMovieOptions
{
    internal bool RecordTransitionLifecycle { get; set; }
    internal int TransitionActivationFrame { get; set; } = 1;
    internal FakeTransitionPlayer? TransitionPlayer { get; set; }
    internal string? TransitionStartLabel { get; set; }
}

internal sealed class FakeLingoMovieHarness
{
    internal FakeLingoMovieHarness(LingoMovie movie, FakeSprite2DManager sprite2DManager, FakeTransitionManager transitionManager, FakeTransitionPlayer transitionPlayer)
    {
        Movie = movie;
        Sprite2DManager = sprite2DManager;
        TransitionManager = transitionManager;
        TransitionPlayer = transitionPlayer;
    }

    internal LingoMovie Movie { get; }
    internal FakeSprite2DManager Sprite2DManager { get; }
    internal FakeTransitionManager TransitionManager { get; }
    internal FakeTransitionPlayer TransitionPlayer { get; }
}
