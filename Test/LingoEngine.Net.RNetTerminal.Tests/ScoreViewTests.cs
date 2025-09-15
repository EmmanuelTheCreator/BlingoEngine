using System;
using System.Reflection;
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

    [Fact]
    public void Redraw_WithNegativeOffset_DoesNotThrowAndClamps()
    {
        Application.Init(new FakeDriver());
        try
        {
            var view = new ScoreView
            {
                Frame = new Rect(0, 0, 20, 10),
                ContentOffset = new Point(-5, -5)
            };

            var act = () => view.Redraw(view.Bounds);
            act.Should().NotThrow();
            view.ContentOffset.X.Should().BeGreaterThanOrEqualTo(0);
            view.ContentOffset.Y.Should().BeGreaterThanOrEqualTo(0);
        }
        finally
        {
            Application.Shutdown();
        }
    }

    [Fact]
    public void MoveCursor_Down_DoesNotThrow()
    {
        Application.Init(new FakeDriver());
        try
        {
            var view = new ScoreView
            {
                Frame = new Rect(0, 0, 20, 10)
            };
            view.LayoutSubviews();
            var move = typeof(ScoreView).GetMethod("MoveCursor", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var act = () => move.Invoke(view, new object[] { 0, 100 });
            act.Should().NotThrow();
        }
        finally
        {
            Application.Shutdown();
        }
    }
}
