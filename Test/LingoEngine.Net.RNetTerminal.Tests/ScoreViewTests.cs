using System;
using Terminal.Gui;
using FluentAssertions;

namespace LingoEngine.Net.RNetTerminal.Tests;

public class ScoreViewTests
{
    [Fact]
    public void MouseEvent_OnScrollBar_DoesNotThrow()
    {
        Application.Init(new FakeDriver());
        try
        {
            var view = new ScoreView
            {
                Frame = new Rect(0, 0, 20, 10)
            };
            var me = new MouseEvent
            {
                X = 19,
                Y = 1,
                Flags = MouseFlags.Button1Clicked
            };

            var act = () => view.MouseEvent(me);
            act.Should().NotThrow();
        }
        finally
        {
            Application.Shutdown();
        }
    }
}
