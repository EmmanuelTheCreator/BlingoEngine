using System.Collections.Generic;
using System.Linq;
using LingoEngine.Events;
using LingoEngine.Lingo.Tests.TestDoubles;
using Xunit;

namespace LingoEngine.Lingo.Tests;

public class TransitionEventOrderTests
{
    [Fact]
    public void AdvanceFrame_RaisesTransitionLifecycleAroundFrameHandlers()
    {
        var timeline = new List<string>();
        var mediator = new LingoEventMediator();
        var frameHandler = new RecordingFrameHandler(timeline);
        mediator.Subscribe(frameHandler);
        mediator.SubscribeStepFrame(frameHandler);

        var harness = FakeLingoMovieBuilder.Create(mediator, timeline, options =>
        {
            options.RecordTransitionLifecycle = true;
            options.TransitionActivationFrame = 1;
        });

        harness.TransitionPlayer.StartResult = false;
        PrivateFieldSetter.SetField(harness.Movie, "_isPlaying", true);

        harness.Movie.AdvanceFrame();
        harness.Movie.OnIdle(1f / 60f);
        harness.Movie.AdvanceFrame();

        var expected = new[]
        {
            "beginSprite",
            "transition.beginSprite",
            "stepFrame",
            "prepareFrame",
            "enterFrame",
            "idleFrame",
            "exitFrame",
            "endSprite",
            "transition.endSprite"
        };

        Assert.True(timeline.Count >= expected.Length, "timeline missing expected callbacks");
        Assert.Equal(expected, timeline.Take(expected.Length));
        Assert.Equal(1, harness.TransitionPlayer.StartCallCount);
    }
}
