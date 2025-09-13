using System;
using System.Collections.Generic;
using Terminal.Gui;

namespace LingoEngine.Director.Client.ConsoleTest;

internal sealed class ScoreView : View
{
    private const int ChannelCount = 100;
    private const int FrameCount = 600;
    private const int LabelWidth = 4;

    private int _cursorChannel;
    private int _cursorFrame;
    private int _vOffset;
    private int _hOffset;
    private readonly List<SpriteBlock> _sprites = new()
    {
        new SpriteBlock(1, 1, 60),
        new SpriteBlock(2, 1, 60),
        new SpriteBlock(3, 1, 60),
        new SpriteBlock(4, 1, 60),
        new SpriteBlock(5, 1, 60)
    };

    public ScoreView()
    {
        CanFocus = true;
    }

    public override bool ProcessKey(KeyEvent keyEvent)
    {
        var step = 1;
        var key = keyEvent.Key;
        var ctrl = (key & Key.CtrlMask) != 0;
        var shift = (key & Key.ShiftMask) != 0;
        var keyNoMods = key & ~Key.CtrlMask & ~Key.ShiftMask & ~Key.AltMask;
        if (ctrl && shift)
        {
            step = 20;
        }
        else if (ctrl)
        {
            step = 10;
        }

        switch (keyNoMods)
        {
            case Key.CursorUp:
                MoveCursor(0, -step);
                return true;
            case Key.CursorDown:
                MoveCursor(0, step);
                return true;
            case Key.CursorLeft:
                MoveCursor(-step, 0);
                return true;
            case Key.CursorRight:
                MoveCursor(step, 0);
                return true;
        }
        return base.ProcessKey(keyEvent);
    }

    private void MoveCursor(int dx, int dy)
    {
        _cursorFrame = Math.Clamp(_cursorFrame + dx, 0, FrameCount - 1);
        _cursorChannel = Math.Clamp(_cursorChannel + dy, 0, ChannelCount - 1);
        EnsureVisible();
        SetNeedsDisplay();
    }

    private void EnsureVisible()
    {
        var visibleFrames = Bounds.Width - LabelWidth;
        var visibleChannels = Bounds.Height - 1;
        if (_cursorFrame < _hOffset)
        {
            _hOffset = _cursorFrame;
        }
        else if (_cursorFrame >= _hOffset + visibleFrames)
        {
            _hOffset = _cursorFrame - visibleFrames + 1;
        }
        if (_cursorChannel < _vOffset)
        {
            _vOffset = _cursorChannel;
        }
        else if (_cursorChannel >= _vOffset + visibleChannels)
        {
            _vOffset = _cursorChannel - visibleChannels + 1;
        }
    }

    public override void Redraw(Rect bounds)
    {
        base.Redraw(bounds);
        var w = Bounds.Width;
        var h = Bounds.Height;
        Driver.SetAttribute(ColorScheme.Normal);
        for (var y = 0; y < h; y++)
        {
            Move(0, y);
            for (var x = 0; x < w; x++)
            {
                Driver.AddRune(' ');
            }
        }

        var visibleFrames = w - LabelWidth;
        var visibleChannels = h - 1;

        for (var i = 0; i < visibleChannels && _vOffset + i < ChannelCount; i++)
        {
            var channel = _vOffset + i + 1;
            Move(0, i + 1);
            var label = channel.ToString().PadLeft(LabelWidth - 1);
            Driver.AddStr(label + "|");
        }

        for (var f = 0; f < visibleFrames && _hOffset + f < FrameCount; f++)
        {
            var frame = _hOffset + f + 1;
            if (frame % 10 == 0)
            {
                var label = frame.ToString();
                var pos = LabelWidth + f - label.Length + 1;
                if (pos >= LabelWidth)
                {
                    Move(pos, 0);
                    Driver.AddStr(label);
                }
            }
        }

        foreach (var sprite in _sprites)
        {
            var channelIdx = sprite.Channel - 1;
            if (channelIdx < _vOffset || channelIdx >= _vOffset + visibleChannels)
            {
                continue;
            }
            var start = Math.Max(sprite.Start - 1, _hOffset);
            var end = Math.Min(sprite.End - 1, _hOffset + visibleFrames - 1);
            if (end < _hOffset || start > _hOffset + visibleFrames - 1)
            {
                continue;
            }
            var y = channelIdx - _vOffset + 1;
            Driver.SetAttribute(Application.Driver.MakeAttribute(Color.Black, Color.BrightBlue));
            for (var f = start; f <= end; f++)
            {
                Move(LabelWidth + f - _hOffset, y);
                Driver.AddRune(' ');
            }
            Driver.SetAttribute(ColorScheme.Normal);
        }

        var cursorX = LabelWidth + _cursorFrame - _hOffset;
        var cursorY = _cursorChannel - _vOffset + 1;
        if (cursorX >= LabelWidth && cursorX < w && cursorY >= 1 && cursorY < h)
        {
            Driver.SetAttribute(Application.Driver.MakeAttribute(Color.BrightRed, Color.Black));
            Move(cursorX, cursorY);
            Driver.AddRune('X');
            Driver.SetAttribute(ColorScheme.Normal);
        }
    }

    private sealed record SpriteBlock(int Channel, int Start, int End);
}
