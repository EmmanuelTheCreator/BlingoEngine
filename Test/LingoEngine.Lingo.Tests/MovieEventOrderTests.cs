using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using AbstUI.Primitives;
using LingoEngine.Core;
using LingoEngine.Events;
using LingoEngine.Inputs;
using LingoEngine.Members;
using LingoEngine.Movies;
using LingoEngine.Movies.Events;
using LingoEngine.Sprites;
using LingoEngine.Transitions;
using Xunit;

namespace LingoEngine.Lingo.Tests;

public class MovieEventOrderTests
{
    [Fact]
    public void AdvanceFrame_RaisesFrameLifecycleEventsInManualOrder()
    {
        var timeline = new List<string>();
        var mediator = new LingoEventMediator();
        var frameHandler = new RecordingFrameHandler(timeline);
        mediator.Subscribe(frameHandler);
        mediator.SubscribeStepFrame(frameHandler);

        var movie = MovieBuilder.Create(mediator, timeline);

        SetField(movie, "_isPlaying", true);

        movie.AdvanceFrame();
        movie.OnIdle(1f / 60f);
        movie.AdvanceFrame();

        var expected = new[]
        {
            "beginSprite",
            "stepFrame",
            "prepareFrame",
            "enterFrame",
            "idleFrame",
            "exitFrame",
            "endSprite"
        };

        Assert.True(timeline.Count >= expected.Length, "timeline missing expected callbacks");
        Assert.Equal(expected, timeline.Take(expected.Length));
    }

    private sealed class RecordingFrameHandler : IHasStepFrameEvent, IHasPrepareFrameEvent,
        IHasEnterFrameEvent, IHasIdleFrameEvent, IHasExitFrameEvent
    {
        private readonly List<string> _timeline;

        internal RecordingFrameHandler(List<string> timeline) => _timeline = timeline;

        public void StepFrame() => _timeline.Add("stepFrame");

        public void PrepareFrame() => _timeline.Add("prepareFrame");

        public void EnterFrame() => _timeline.Add("enterFrame");

        public void IdleFrame() => _timeline.Add("idleFrame");

        public void ExitFrame() => _timeline.Add("exitFrame");
    }

    private sealed class FakeTransitionPlayer : ILingoTransitionPlayer
    {
        public bool IsActive => false;

        public bool Start(LingoTransitionSprite sprite) => false;

        public void Tick() { }

        public void Dispose() { }
    }

    private sealed class FakeFrameworkMovie : ILingoFrameworkMovie
    {
        public string Name { get; set; } = "TestFrameworkMovie";
        public bool Visibility { get; set; } = true;
        public float Width { get; set; } = 640f;
        public float Height { get; set; } = 480f;
        public AMargin Margin { get; set; } = AMargin.Zero;
        public int ZIndex { get; set; }
        public object FrameworkNode => this;

        public void Dispose() { }

        public APoint GetGlobalMousePosition() => (0, 0);

        public void RemoveMe() { }

        public void UpdateStage() { }
    }

    private sealed class FakeSprite2DManager : LingoSprite2DManager
    {
        private List<string> _timeline = null!;
        private bool _shouldBegin;
        private bool _shouldEnd;

        private FakeSprite2DManager() : base(null!, null!)
        {
        }

        internal static FakeSprite2DManager Create(LingoMovie movie, LingoStageMouse mouse, List<string> timeline)
        {
            var manager = (FakeSprite2DManager)FormatterServices.GetUninitializedObject(typeof(FakeSprite2DManager));
            manager.Initialize(movie, mouse, timeline);
            return manager;
        }

        private void Initialize(LingoMovie movie, LingoStageMouse mouse, List<string> timeline)
        {
            _timeline = timeline;
            _shouldBegin = false;
            _shouldEnd = false;

            SetField(this, "_movie", movie);
            SetField(this, "_environment", null);
            SetField(this, "_lingoMouse", mouse);
            SetField(this, "_mutedSprites", new List<int>());
            SpriteNumChannelOffset = LingoSprite2D.SpriteNumOffset;

            // Provide empty collections so any incidental access from base code
            // does not crash during the test.
            SetField(this, "_spriteChannels", new Dictionary<int, LingoSpriteChannel>());
            SetField(this, "_spritesByName", new Dictionary<string, LingoSprite2D>());
            SetField(this, "_allTimeSprites", new List<LingoSprite2D>());
            SetField(this, "_newPuppetSprites", new List<LingoSprite2D>());
            SetField(this, "_activeSprites", new Dictionary<int, LingoSprite2D>());
            SetField(this, "_activeSpritesOrdered", new List<LingoSprite2D>());
            SetField(this, "_enteredSprites", new List<LingoSprite2D>());
            SetField(this, "_exitedSprites", new List<LingoSprite2D>());
            SetField(this, "_deletedPuppetSpritesCache", new Dictionary<int, LingoSprite2D>());
            SetField(this, "_changedMembers", new List<LingoMember>());
        }

        internal override void UpdateActiveSprites(int currentFrame, int lastFrame)
        {
            if (!_shouldBegin && currentFrame >= 1)
            {
                _shouldBegin = true;
                _shouldEnd = true;
            }
        }

        internal override void BeginSprites()
        {
            if (_shouldBegin)
            {
                _timeline.Add("beginSprite");
                _shouldBegin = false;
            }
        }

        internal override void PrepareEndSprites()
        {
            // No-op; endSprite is dispatched once exitFrame runs.
        }

        internal override void DispatchEndSprites()
        {
            if (_shouldEnd)
            {
                _timeline.Add("endSprite");
                _shouldEnd = false;
            }
        }
    }

    private sealed class MovieBuilder
    {
        internal static LingoMovie Create(LingoEventMediator mediator, List<string> timeline)
        {
            var movie = (LingoMovie)FormatterServices.GetUninitializedObject(typeof(LingoMovie));

            SetField(movie, "_eventMediator", mediator);
            SetField(movie, "_spriteManagers", new List<LingoSpriteManager>());
            SetField(movie, "_actorList", new ActorList());
            SetField(movie, "_idleHandlerPeriod", 1);
            SetField(movie, "_idleIntervalSeconds", 1f / 60f);
            SetField(movie, "_currentFrame", 0);
            SetField(movie, "_lastFrame", 0);
            SetField(movie, "_nextFrame", -1);
            SetField(movie, "_needToRaiseStartMovie", false);
            SetField(movie, "_hasPendingEndSprites", false);
            SetField(movie, "_frameIsActive", false);
            SetField(movie, "_idleAccumulator", 0f);

            var mouse = (LingoStageMouse)FormatterServices.GetUninitializedObject(typeof(LingoStageMouse));
            SetField(movie, "_lingoMouse", mouse);

            var frameworkMovie = new FakeFrameworkMovie();
            SetField(movie, "_frameworkMovie", frameworkMovie);

            var transitionManager = (LingoSpriteTransitionManager)FormatterServices.GetUninitializedObject(typeof(LingoSpriteTransitionManager));
            InitializeTransitionManager(transitionManager, movie);
            SetField(movie, "_transitionManager", transitionManager);

            var transitionPlayer = new FakeTransitionPlayer();
            SetField(movie, "_transitionPlayer", transitionPlayer);

            var spriteManager = FakeSprite2DManager.Create(movie, mouse, timeline);
            SetField(movie, "_sprite2DManager", spriteManager);

            return movie;
        }

        private static void InitializeTransitionManager(LingoSpriteTransitionManager manager, LingoMovie movie)
        {
            SetField(manager, "_movie", movie);
            SetField(manager, "_environment", null);
            SetField(manager, "_mutedSprites", new List<int>());
            SetField(manager, "_spriteChannels", new Dictionary<int, LingoSpriteChannel>());
            SetField(manager, "_spritesByName", new Dictionary<string, LingoTransitionSprite>());
            SetField(manager, "_allTimeSprites", new List<LingoTransitionSprite>());
            SetField(manager, "_newPuppetSprites", new List<LingoTransitionSprite>());
            SetField(manager, "_activeSprites", new Dictionary<int, LingoTransitionSprite>());
            SetField(manager, "_activeSpritesOrdered", new List<LingoTransitionSprite>());
            SetField(manager, "_enteredSprites", new List<LingoTransitionSprite>());
            SetField(manager, "_exitedSprites", new List<LingoTransitionSprite>());
            SetField(manager, "_deletedPuppetSpritesCache", new Dictionary<int, LingoTransitionSprite>());
        }
    }

    private static void SetField(object target, string fieldName, object? value)
    {
        var type = target.GetType();
        while (type != null)
        {
            var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field != null)
            {
                field.SetValue(target, value);
                return;
            }

            type = type.BaseType;
        }

        throw new InvalidOperationException($"Field '{fieldName}' not found on type '{target.GetType()}'.");
    }
}
