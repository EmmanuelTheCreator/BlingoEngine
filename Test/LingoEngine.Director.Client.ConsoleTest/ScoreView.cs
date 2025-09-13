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
    private int _playFrame;
    private readonly List<SpriteBlock> _sprites = new()
    {
        new SpriteBlock(1, 1, 60, 1, "Greeting"),
        new SpriteBlock(2, 1, 60, 2, "Info"),
        new SpriteBlock(3, 1, 60, 3, "Box"),
        new SpriteBlock(4, 1, 60, 4, "Greeting"),
        new SpriteBlock(5, 1, 60, 5, "Info")
    };

    public ScoreView()
    {
        CanFocus = true;
    }

    public event Action<int, int, int?, string?>? InfoChanged;

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
            case Key.Enter:
                ShowActionMenu();
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
        NotifyInfoChanged();
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

        if (_playFrame >= _hOffset && _playFrame < _hOffset + visibleFrames)
        {
            var frame = _playFrame + 1;
            var label = frame.ToString();
            var pos = LabelWidth + _playFrame - _hOffset - label.Length + 1;
            if (pos >= LabelWidth)
            {
                Driver.SetAttribute(Application.Driver.MakeAttribute(Color.BrightRed, Color.Blue));
                Move(pos, 0);
                Driver.AddStr(label);
                Driver.SetAttribute(ColorScheme.Normal);
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
            Driver.SetAttribute(Application.Driver.MakeAttribute(Color.BrightGreen, Color.Black));
            Move(cursorX, cursorY);
            Driver.AddRune('X');
            Driver.SetAttribute(ColorScheme.Normal);
        }
    }

    public void SetPlayFrame(int frame)
    {
        _playFrame = Math.Clamp(frame, 0, FrameCount - 1);
        SetNeedsDisplay();
    }

    private void ShowActionMenu()
    {
        var sprite = FindSprite(_cursorChannel + 1, _cursorFrame + 1);
        string[] items;
        if (sprite == null)
        {
            items = new[] { "Create Sprite" };
        }
        else
        {
            items = new[]
            {
                "Delete Sprite",
                "Add Keyframe",
                "Move Keyframe",
                "Change Start",
                "Change End",
                "Play From Here",
                "Select Sprite"
            };
        }

        var list = new ListView(items)
        {
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        var dialog = new Dialog("Score", 30, items.Length + 4);
        dialog.Add(list);
        list.OpenSelectedItem += args =>
        {
            var choice = items[args.Item];
            Application.RequestStop();
            HandleAction(choice, sprite);
        };
        Application.Run(dialog);
    }

    private void HandleAction(string action, SpriteBlock? sprite)
    {
        switch (action)
        {
            case "Create Sprite":
                CreateSpriteDialog();
                break;
            case "Delete Sprite":
                if (sprite != null)
                {
                    _sprites.Remove(sprite);
                    SetNeedsDisplay();
                    NotifyInfoChanged();
                }
                break;
            case "Add Keyframe":
                PromptForInt("Add Keyframe", "Frame:");
                break;
            case "Move Keyframe":
                PromptForInt("Move Keyframe", "Frame:");
                break;
            case "Change Start":
                if (sprite != null)
                {
                    var val = PromptForInt("Change Start", "Start:");
                    if (val.HasValue)
                    {
                        sprite.Start = val.Value;
                        SetNeedsDisplay();
                        NotifyInfoChanged();
                    }
                }
                break;
            case "Change End":
                if (sprite != null)
                {
                    var val = PromptForInt("Change End", "End:");
                    if (val.HasValue)
                    {
                        sprite.End = val.Value;
                        SetNeedsDisplay();
                        NotifyInfoChanged();
                    }
                }
                break;
            case "Play From Here":
                PlayFromHere?.Invoke(_cursorFrame + 1);
                break;
            case "Select Sprite":
                if (sprite != null)
                {
                    SpriteSelected?.Invoke(sprite.Channel, sprite.Start);
                    NotifyInfoChanged();
                }
                break;
        }
    }

    private void CreateSpriteDialog()
    {
        var begin = new TextField("1") { X = 14, Y = 1, Width = 10 };
        var end = new TextField("1") { X = 14, Y = 3, Width = 10 };
        var locH = new TextField("0") { X = 14, Y = 5, Width = 10 };
        var locV = new TextField("0") { X = 14, Y = 7, Width = 10 };
        var member = new TextField("member") { X = 14, Y = 9, Width = 20 };
        var ok = new Button("Ok", true);
        ok.Clicked += () =>
        {
            if (int.TryParse(begin.Text.ToString(), out var b) &&
                int.TryParse(end.Text.ToString(), out var e))
            {
                var num = _sprites.Count + 1;
                _sprites.Add(new SpriteBlock(_cursorChannel + 1, b, e, num, member.Text.ToString()));
                SetNeedsDisplay();
                NotifyInfoChanged();
            }
            Application.RequestStop();
        };
        var dialog = new Dialog("Create Sprite", 40, 15, ok);
        dialog.Add(
            new Label("Begin:") { X = 1, Y = 1 }, begin,
            new Label("End:") { X = 1, Y = 3 }, end,
            new Label("LocH:") { X = 1, Y = 5 }, locH,
            new Label("LocV:") { X = 1, Y = 7 }, locV,
            new Label("MemberName:") { X = 1, Y = 9 }, member);
        Application.Run(dialog);
    }

    private int? PromptForInt(string title, string prompt)
    {
        int? result = null;
        var field = new TextField("0") { X = 12, Y = 1, Width = 10 };
        var ok = new Button("Ok", true);
        ok.Clicked += () =>
        {
            if (int.TryParse(field.Text.ToString(), out var v))
            {
                result = v;
            }
            Application.RequestStop();
        };
        var dialog = new Dialog(title, 30, 7, ok);
        dialog.Add(new Label(prompt) { X = 1, Y = 1 }, field);
        Application.Run(dialog);
        return result;
    }

    private SpriteBlock? FindSprite(int channel, int frame)
    {
        foreach (var sprite in _sprites)
        {
            if (sprite.Channel == channel && frame >= sprite.Start && frame <= sprite.End)
            {
                return sprite;
            }
        }
        return null;
    }

    private void NotifyInfoChanged()
    {
        var sprite = FindSprite(_cursorChannel + 1, _cursorFrame + 1);
        InfoChanged?.Invoke(_cursorFrame + 1, _cursorChannel + 1, sprite?.Number, sprite?.MemberName);
    }

    public void TriggerInfo() => NotifyInfoChanged();

    public event Action<int, int>? SpriteSelected;

    public event Action<int>? PlayFromHere;

    private sealed class SpriteBlock
    {
        public int Channel { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
        public int Number { get; }
        public string MemberName { get; }

        public SpriteBlock(int channel, int start, int end, int number, string memberName)
        {
            Channel = channel;
            Start = start;
            End = end;
            Number = number;
            MemberName = memberName;
        }
    }
}
