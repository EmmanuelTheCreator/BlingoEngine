using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using LingoEngine.Events;
using LingoEngine.Lingo.Tests.TestDoubles;
using LingoEngine.Members;
using LingoEngine.Movies;
using LingoEngine.Sprites;
using AbstUI.Components;
using AbstUI.Primitives;
using Xunit;

namespace LingoEngine.Lingo.Tests;

public sealed class PuppetSpriteLifecycleTests
{
    [Fact]
    public void UpdateActiveSprites_RemovesTimelineSpriteWhenPuppetNotSet()
    {
        var harness = TestHarness.Create();
        var sprite = harness.CreateSprite(spriteNum: 1, beginFrame: 1, endFrame: 1);
        sprite.IsActive = true;
        harness.Manager.TrackActiveSprite(sprite);

        harness.Manager.UpdateActiveSprites(currentFrame: 2, lastFrame: 1);

        Assert.DoesNotContain(sprite, harness.Manager.ActiveSprites);
        Assert.Contains(sprite, harness.Manager.ExitedSprites);
        Assert.False(sprite.IsActive);
    }

    [Fact]
    public void UpdateActiveSprites_PreservesSpriteWhenPuppetTrue()
    {
        var harness = TestHarness.Create();
        var sprite = harness.CreateSprite(spriteNum: 1, beginFrame: 1, endFrame: 1);
        sprite.IsActive = true;
        sprite.Puppet = true;
        harness.Manager.TrackActiveSprite(sprite);

        harness.Manager.UpdateActiveSprites(currentFrame: 2, lastFrame: 1);

        Assert.Contains(sprite, harness.Manager.ActiveSprites);
        Assert.DoesNotContain(sprite, harness.Manager.ExitedSprites);
        Assert.True(sprite.IsActive);
    }

    private sealed class TestHarness
    {
        private TestHarness(TestSpriteManager manager, LingoEventMediator mediator)
        {
            Manager = manager;
            Mediator = mediator;
        }

        internal TestSpriteManager Manager { get; }
        private LingoEventMediator Mediator { get; }

        internal static TestHarness Create()
        {
            var mediator = new LingoEventMediator();
            var manager = TestSpriteManager.Create();
            return new TestHarness(manager, mediator);
        }

        internal TestSprite CreateSprite(int spriteNum, int beginFrame, int endFrame)
        {
            var sprite = new TestSprite(Mediator);
            sprite.Initialize(spriteNum, beginFrame, endFrame);
            return sprite;
        }
    }

    private sealed class TestSpriteManager : LingoSpriteManager<TestSprite>
    {
        private TestSpriteManager() : base(0, null!, null!)
        {
        }

        internal static TestSpriteManager Create()
        {
            var manager = (TestSpriteManager)FormatterServices.GetUninitializedObject(typeof(TestSpriteManager));
            manager.Initialize();
            return manager;
        }

        private void Initialize()
        {
            PrivateFieldSetter.SetField(this, "_mutedSprites", new List<int>());
            PrivateFieldSetter.SetField(this, "_movie", (LingoMovie)FormatterServices.GetUninitializedObject(typeof(LingoMovie)));
            PrivateFieldSetter.SetField(this, "_environment", null);
            PrivateFieldSetter.SetField(this, "<SpriteNumChannelOffset>k__BackingField", 0);
            PrivateFieldSetter.SetField(this, "_spriteChannels", new Dictionary<int, LingoSpriteChannel>());
            PrivateFieldSetter.SetField(this, "_spritesByName", new Dictionary<string, TestSprite>());
            PrivateFieldSetter.SetField(this, "_allTimeSprites", new List<TestSprite>());
            PrivateFieldSetter.SetField(this, "_newPuppetSprites", new List<TestSprite>());
            PrivateFieldSetter.SetField(this, "_activeSprites", new Dictionary<int, TestSprite>());
            PrivateFieldSetter.SetField(this, "_activeSpritesOrdered", new List<TestSprite>());
            PrivateFieldSetter.SetField(this, "_enteredSprites", new List<TestSprite>());
            PrivateFieldSetter.SetField(this, "_exitedSprites", new List<TestSprite>());
            PrivateFieldSetter.SetField(this, "_deletedPuppetSpritesCache", new Dictionary<int, TestSprite>());
        }

        protected override TestSprite OnCreateSprite(LingoMovie movie, Action<TestSprite> onRemove) => throw new NotSupportedException();

        protected override LingoSprite? OnAdd(int spriteNum, int begin, int end, ILingoMember? member) => null;

        protected override void SpriteEntered(TestSprite sprite)
        {
        }

        protected override void SpriteExited(TestSprite sprite)
        {
        }

        internal void TrackActiveSprite(TestSprite sprite)
        {
            _allTimeSprites.Add(sprite);
            _activeSprites[sprite.SpriteNum] = sprite;
            _activeSpritesOrdered.Add(sprite);
        }

        internal IReadOnlyCollection<TestSprite> ActiveSprites => _activeSprites.Values;
        internal IReadOnlyCollection<TestSprite> ExitedSprites => _exitedSprites;
    }

    private sealed class TestSprite : LingoSprite
    {
        private readonly StubFrameworkSprite _stubFrameworkSprite;

        internal TestSprite(LingoEventMediator mediator) : base(mediator)
        {
            _stubFrameworkSprite = new StubFrameworkSprite();
            PrivateFieldSetter.SetField(this, "_frameworkSprite", _stubFrameworkSprite);
        }

        public override int SpriteNumWithChannel => SpriteNum;

        internal void Initialize(int spriteNum, int beginFrame, int endFrame)
        {
            Init(spriteNum, $"Sprite_{spriteNum}");
            BeginFrame = beginFrame;
            EndFrame = endFrame;
        }

        public override void OnRemoveMe()
        {
        }
    }

    private sealed class StubFrameworkSprite : ILingoFrameworkSprite
    {
        public string Name { get; set; } = string.Empty;
        public bool Visibility { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public AMargin Margin { get; set; }
        public int ZIndex { get; set; }
        public object FrameworkNode => this;
        public float X { get; set; }
        public float Y { get; set; }
        public float Blend { get; set; }
        public APoint RegPoint { get; set; }
        public float DesiredHeight { get; set; }
        public float DesiredWidth { get; set; }
        public float Rotation { get; set; }
        public float Skew { get; set; }
        public bool FlipH { get; set; }
        public bool FlipV { get; set; }
        public bool DirectToStage { get; set; }
        public int Ink { get; set; }

        public void Dispose()
        {
        }

        public void Hide()
        {
        }

        public void MemberChanged()
        {
        }

        public void RemoveMe()
        {
        }

        public void SetPosition(APoint point)
        {
            X = point.X;
            Y = point.Y;
        }

        public void SetTexture(IAbstTexture2D texture)
        {
        }

        public void Show()
        {
        }

        public void ApplyMemberChangesOnStepFrame()
        {
        }
    }
}
