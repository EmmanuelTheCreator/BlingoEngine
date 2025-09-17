using System.Collections.Generic;
using LingoEngine.Movies.Events;

namespace LingoEngine.Lingo.Tests.TestDoubles;

internal sealed class RecordingFrameHandler : IHasStepFrameEvent, IHasPrepareFrameEvent,
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
