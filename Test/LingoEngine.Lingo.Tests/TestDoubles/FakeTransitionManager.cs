using System.Collections.Generic;
using System.Runtime.Serialization;
using LingoEngine.Events;
using LingoEngine.Movies;
using LingoEngine.Sprites;
using LingoEngine.Transitions;

namespace LingoEngine.Lingo.Tests.TestDoubles;

internal sealed class FakeTransitionManager : LingoSpriteTransitionManager
{
    private List<string> _timeline = null!;
    private bool _shouldBegin;
    private bool _shouldEnd;
    private bool _hasActivated;
    private int _activationFrame;
    private bool _recordLifecycle;
    private LingoTransitionSprite? _fakeSprite;

    private FakeTransitionManager() : base(null!, null!)
    {
    }

    internal static FakeTransitionManager Create(LingoMovie movie, LingoEventMediator mediator, List<string> timeline)
    {
        var manager = (FakeTransitionManager)FormatterServices.GetUninitializedObject(typeof(FakeTransitionManager));
        manager.Initialize(movie, mediator, timeline);
        return manager;
    }

    private void Initialize(LingoMovie movie, LingoEventMediator mediator, List<string> timeline)
    {
        _timeline = timeline;
        _shouldBegin = false;
        _shouldEnd = false;
        _hasActivated = false;
        _activationFrame = int.MaxValue;
        _recordLifecycle = false;

        PrivateFieldSetter.SetField(this, "_movie", movie);
        PrivateFieldSetter.SetField(this, "_environment", null);
        PrivateFieldSetter.SetField(this, "_mutedSprites", new List<int>());
        SpriteNumChannelOffset = LingoTransitionSprite.SpriteNumOffset;

        PrivateFieldSetter.SetField(this, "_spriteChannels", new Dictionary<int, LingoSpriteChannel>());
        PrivateFieldSetter.SetField(this, "_spritesByName", new Dictionary<string, LingoTransitionSprite>());
        PrivateFieldSetter.SetField(this, "_allTimeSprites", new List<LingoTransitionSprite>());
        PrivateFieldSetter.SetField(this, "_newPuppetSprites", new List<LingoTransitionSprite>());
        PrivateFieldSetter.SetField(this, "_activeSprites", new Dictionary<int, LingoTransitionSprite>());
        PrivateFieldSetter.SetField(this, "_activeSpritesOrdered", new List<LingoTransitionSprite>());
        PrivateFieldSetter.SetField(this, "_enteredSprites", new List<LingoTransitionSprite>());
        PrivateFieldSetter.SetField(this, "_exitedSprites", new List<LingoTransitionSprite>());
        PrivateFieldSetter.SetField(this, "_deletedPuppetSpritesCache", new Dictionary<int, LingoTransitionSprite>());

        _fakeSprite = (LingoTransitionSprite)FormatterServices.GetUninitializedObject(typeof(LingoTransitionSprite));
        PrivateFieldSetter.SetField(_fakeSprite, "_eventMediator", mediator);
        _fakeSprite.BeginFrame = 1;
        _fakeSprite.EndFrame = 1;
    }

    internal bool IsLifecycleRecordingEnabled
    {
        get => _recordLifecycle;
        set
        {
            _recordLifecycle = value;
            if (!value)
            {
                _activationFrame = int.MaxValue;
                _hasActivated = false;
                _shouldBegin = false;
                _shouldEnd = false;
            }
        }
    }

    internal void SetActivationFrame(int frame)
    {
        _activationFrame = frame;
        _hasActivated = false;
        _shouldBegin = false;
        _shouldEnd = false;
        if (_fakeSprite != null)
        {
            _fakeSprite.BeginFrame = frame;
            _fakeSprite.EndFrame = frame;
        }

        if (frame == int.MaxValue)
        {
            _allTimeSprites.Clear();
        }
        else if (_fakeSprite != null)
        {
            _allTimeSprites.Clear();
            _allTimeSprites.Add(_fakeSprite);
        }
    }

    internal override void UpdateActiveSprites(int currentFrame, int lastFrame)
    {
        if (!_recordLifecycle || _hasActivated)
            return;

        if (currentFrame >= _activationFrame)
        {
            _shouldBegin = true;
            _shouldEnd = true;
            _hasActivated = true;
        }
    }

    internal override void BeginSprites()
    {
        if (!_recordLifecycle)
            return;

        if (_shouldBegin)
        {
            _timeline.Add("transition.beginSprite");
            _shouldBegin = false;
        }
    }

    internal override void PrepareEndSprites()
    {
    }

    internal override void DispatchEndSprites()
    {
        if (!_recordLifecycle)
            return;

        if (_shouldEnd)
        {
            _timeline.Add("transition.endSprite");
            _shouldEnd = false;
        }
    }
}
