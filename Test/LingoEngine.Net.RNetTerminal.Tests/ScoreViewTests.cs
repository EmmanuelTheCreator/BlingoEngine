using System;
using System.Reflection;
using Terminal.Gui;
using FluentAssertions;

namespace LingoEngine.Net.RNetTerminal.Tests;

public class ScoreViewTests
{
    private static void SetPrivateField(object obj, string name, object value)
        => obj.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(obj, value);

    private static T GetPrivateField<T>(object obj, string name)
        => (T)obj.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(obj)!;

    private static Point GetOffset(ScoreView view)
        => (Point)view.GetType().GetMethod("GetOffset", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(view, null)!;
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

    [Theory]
    [InlineData(Key.CursorUp, "_cursorChannel", -1)]
    [InlineData(Key.CursorDown, "_cursorChannel", 1)]
    [InlineData(Key.CursorLeft, "_cursorFrame", -1)]
    [InlineData(Key.CursorRight, "_cursorFrame", 1)]
    public void ProcessKey_MovesCursor_WhenNotOverSprite(Key key, string field, int delta)
    {
        Application.Init(new FakeDriver());
        try
        {
            var view = new ScoreView
            {
                Frame = new Rect(0, 0, 20, 10)
            };
            view.LayoutSubviews();
            SetPrivateField(view, "_cursorFrame", 1);
            SetPrivateField(view, "_cursorChannel", 1);
            var before = GetPrivateField<int>(view, field);
            view.ProcessKey(new KeyEvent(key, new KeyModifiers()));
            var after = GetPrivateField<int>(view, field);
            after.Should().Be(before + delta);
        }
        finally
        {
            Application.Shutdown();
        }
    }

    [Theory]
    [InlineData(Key.CursorUp, "_cursorChannel", -1)]
    [InlineData(Key.CursorDown, "_cursorChannel", 1)]
    [InlineData(Key.CursorLeft, "_cursorFrame", -1)]
    [InlineData(Key.CursorRight, "_cursorFrame", 1)]
    public void ProcessKey_MovesCursor_WhenOverSprite(Key key, string field, int delta)
    {
        Application.Init(new FakeDriver());
        try
        {
            var view = new ScoreView
            {
                Frame = new Rect(0, 0, 20, 10)
            };
            view.LayoutSubviews();
            SetPrivateField(view, "_cursorFrame", 1);
            SetPrivateField(view, "_cursorChannel", 5);
            var before = GetPrivateField<int>(view, field);
            view.ProcessKey(new KeyEvent(key, new KeyModifiers()));
            var after = GetPrivateField<int>(view, field);
            after.Should().Be(before + delta);
        }
        finally
        {
            Application.Shutdown();
        }
    }

    [Fact]
    public void ProcessKey_RightAtEdge_ScrollsHorizontally()
    {
        Application.Init(new FakeDriver());
        try
        {
            var view = new ScoreView
            {
                Frame = new Rect(0, 0, 20, 10)
            };
            view.LayoutSubviews();
            var labelWidth = GetPrivateField<int>(view, "_labelWidth");
            var visibleFrames = view.Frame.Width - labelWidth - 1;
            SetPrivateField(view, "_cursorFrame", visibleFrames - 1);
            SetPrivateField(view, "_cursorChannel", 1);
            var before = GetOffset(view).X;
            view.ProcessKey(new KeyEvent(Key.CursorRight, new KeyModifiers()));
            var after = GetOffset(view).X;
            after.Should().BeGreaterThan(before);
        }
        finally
        {
            Application.Shutdown();
        }
    }

    [Fact]
    public void ProcessKey_DownAtEdge_ScrollsVertically()
    {
        Application.Init(new FakeDriver());
        try
        {
            var view = new ScoreView
            {
                Frame = new Rect(0, 0, 20, 10)
            };
            view.LayoutSubviews();
            var scrollBarHeight = 1;
            var visibleChannels = view.Frame.Height - 1 - scrollBarHeight;
            SetPrivateField(view, "_cursorChannel", visibleChannels - 1);
            SetPrivateField(view, "_cursorFrame", 1);
            var before = GetOffset(view).Y;
            view.ProcessKey(new KeyEvent(Key.CursorDown, new KeyModifiers()));
            var after = GetOffset(view).Y;
            after.Should().BeGreaterThan(before);
        }
        finally
        {
            Application.Shutdown();
        }
    }

    [Fact]
    public void MouseWheel_ScrollsContent()
    {
        Application.Init(new FakeDriver());
        try
        {
            var view = new ScoreView
            {
                Frame = new Rect(0, 0, 20, 10)
            };
            view.LayoutSubviews();
            var before = GetOffset(view).Y;
            var me = new MouseEvent
            {
                X = 0,
                Y = 0,
                Flags = MouseFlags.WheeledDown
            };
            view.MouseEvent(me);
            var after = GetOffset(view).Y;
            after.Should().BeGreaterThan(before);
        }
        finally
        {
            Application.Shutdown();
        }
    }
}
