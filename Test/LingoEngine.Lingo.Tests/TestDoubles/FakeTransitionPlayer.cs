using System.Collections.Generic;
using LingoEngine.Transitions;

namespace LingoEngine.Lingo.Tests.TestDoubles;

internal sealed class FakeTransitionPlayer : ILingoTransitionPlayer
{
    private readonly List<string>? _timeline;
    private readonly string? _startLabel;

    internal FakeTransitionPlayer(List<string>? timeline = null, string? startLabel = null)
    {
        _timeline = timeline;
        _startLabel = startLabel;
    }

    internal bool StartResult { get; set; }

    internal int StartCallCount { get; private set; }

    public bool IsActive { get; private set; }

    public bool Start(LingoTransitionSprite sprite)
    {
        StartCallCount++;
        if (_startLabel != null)
            _timeline?.Add(_startLabel);

        IsActive = StartResult;
        return StartResult;
    }

    public void Tick()
    {
        if (IsActive)
            _timeline?.Add("transition.tick");
    }

    public void Dispose()
    {
    }
}
