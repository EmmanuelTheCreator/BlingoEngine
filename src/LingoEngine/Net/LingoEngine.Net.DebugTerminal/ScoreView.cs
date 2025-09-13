using System;
using System.Collections.Generic;
using System.Linq;
using Terminal.Gui;

namespace LingoEngine.Net.DebugTerminal;

internal sealed class ScoreView : ScrollView
{
    private const int SpriteChannelCount = 100;
    private const int FrameCount = 600;
    private const int LabelWidth = 4;
    private static readonly string[] SpecialChannels =
    {
        "Tempo",
        "Palette",
        "Script",
        "Sound1",
        "Sound2"
    };
    private static readonly string[] TweenProperties =
    {
        "LocH",
        "LocV",
        "LocZ",
        "Width",
        "Height",
        "Rotation",
        "Skew",
        "Blend",
        "Ink"
    };

    private int _cursorChannel;
    private int _cursorFrame;
    private int _playFrame;
    private readonly List<SpriteBlock> _sprites;
    private int? _selectedSprite;

    private int TotalChannels => SpriteChannelCount + SpecialChannels.Length;

    public ScoreView()
    {
        _sprites = TestMovieBuilder.BuildSprites()
            .Select(s => new SpriteBlock(s.SpriteNum, s.BeginFrame, s.EndFrame, s.SpriteNum, s.Name))
            .ToList();
        CanFocus = true;
        ColorScheme = new ColorScheme
        {
            Normal = Application.Driver.MakeAttribute(Color.White, Color.DarkGray)
        };
        ContentSize = new Size(FrameCount + LabelWidth, TotalChannels + 1);
        ShowVerticalScrollIndicator = true;
        ShowHorizontalScrollIndicator = true;
    }

    public event Action<int, int, int?, string?>? InfoChanged;

    public void RequestRedraw() => SetNeedsDisplay();

    public void SelectSprite(int? spriteNumber)
    {
        _selectedSprite = spriteNumber;
        SetNeedsDisplay();
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
            case Key.Enter:
                ShowActionMenu();
                return true;
        }
        return base.ProcessKey(keyEvent);
    }

    public override bool MouseEvent(MouseEvent me)
    {
        if (me.Flags.HasFlag(MouseFlags.Button1Clicked))
        {
            var frame = ContentOffset.X + me.X - LabelWidth;
            var channel = ContentOffset.Y + me.Y - 1;
            if (frame >= 0 && frame < FrameCount && channel >= 0 && channel < TotalChannels)
            {
                _cursorFrame = frame;
                _cursorChannel = channel;
                EnsureVisible();
                SetNeedsDisplay();
                NotifyInfoChanged();
            }
            ClampContentOffset();
            return true;
        }
        if (me.Flags.HasFlag(MouseFlags.Button3Clicked))
        {
            var frame = ContentOffset.X + me.X - LabelWidth;
            var channel = ContentOffset.Y + me.Y - 1;
            if (frame >= 0 && frame < FrameCount && channel >= 0 && channel < TotalChannels)
            {
                _cursorFrame = frame;
                _cursorChannel = channel;
                EnsureVisible();
                SetNeedsDisplay();
                NotifyInfoChanged();
                ShowActionMenu();
            }
            ClampContentOffset();
            return true;
        }
        var handled = base.MouseEvent(me);
        ClampContentOffset();
        return handled;
    }

    private void ClampContentOffset()
    {
        var offset = ContentOffset;
        var visibleFrames = Bounds.Width - LabelWidth;
        var visibleChannels = Bounds.Height - 1;
        var maxX = Math.Max(0, FrameCount - visibleFrames);
        var maxY = Math.Max(0, TotalChannels - visibleChannels);
        offset.X = Math.Clamp(offset.X, 0, maxX);
        offset.Y = Math.Clamp(offset.Y, 0, maxY);
        ContentOffset = offset;
    }

    private void MoveCursor(int dx, int dy)
    {
        _cursorFrame = Math.Clamp(_cursorFrame + dx, 0, FrameCount - 1);
        _cursorChannel = Math.Clamp(_cursorChannel + dy, 0, TotalChannels - 1);
        EnsureVisible();
        SetNeedsDisplay();
        NotifyInfoChanged();
    }

    private void EnsureVisible()
    {
        var visibleFrames = Bounds.Width - LabelWidth;
        var visibleChannels = Bounds.Height - 1;
        var offset = ContentOffset;
        if (_cursorFrame < offset.X)
        {
            offset.X = _cursorFrame;
        }
        else if (_cursorFrame >= offset.X + visibleFrames)
        {
            offset.X = _cursorFrame - visibleFrames + 1;
        }
        if (_cursorChannel < offset.Y)
        {
            offset.Y = _cursorChannel;
        }
        else if (_cursorChannel >= offset.Y + visibleChannels)
        {
            offset.Y = _cursorChannel - visibleChannels + 1;
        }
        ContentOffset = offset;
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
        var offsetX = ContentOffset.X;
        var offsetY = ContentOffset.Y;

        for (var i = 0; i < visibleChannels && offsetY + i < TotalChannels; i++)
        {
            var channelIndex = offsetY + i;
            Move(0, i + 1);
            string label;
            if (channelIndex < SpecialChannels.Length)
            {
                label = SpecialChannels[channelIndex];
            }
            else
            {
                var chNum = channelIndex - SpecialChannels.Length + 1;
                label = chNum.ToString();
            }
            label = label.PadLeft(LabelWidth - 1);
            Driver.AddStr(label + "|");
        }

        for (var f = 0; f < visibleFrames && offsetX + f < FrameCount; f++)
        {
            var frame = offsetX + f + 1;
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

        if (_playFrame >= offsetX && _playFrame < offsetX + visibleFrames)
        {
            var frame = _playFrame + 1;
            var label = frame.ToString();
            var pos = LabelWidth + _playFrame - offsetX - label.Length + 1;
            if (pos >= LabelWidth)
            {
                Driver.SetAttribute(Application.Driver.MakeAttribute(Color.BrightRed, Color.BrightBlue));
                Move(pos, 0);
                Driver.AddStr(label);
                Driver.SetAttribute(ColorScheme.Normal);
            }
        }

        foreach (var sprite in _sprites)
        {
            var channelIdx = sprite.Channel - 1 + SpecialChannels.Length;
            if (channelIdx < offsetY || channelIdx >= offsetY + visibleChannels)
            {
                continue;
            }
            var start = Math.Max(sprite.Start - 1, offsetX);
            var end = Math.Min(sprite.End - 1, offsetX + visibleFrames - 1);
            if (end < offsetX || start > offsetX + visibleFrames - 1)
            {
                continue;
            }
            var y = channelIdx - offsetY + 1;
            var bg = sprite.Number == _selectedSprite ? Color.Blue : Color.BrightBlue;
            Driver.SetAttribute(Application.Driver.MakeAttribute(Color.White, bg));
            for (var f = start; f <= end; f++)
            {
                Move(LabelWidth + f - offsetX, y);
                Driver.AddRune(' ');
            }
            Driver.SetAttribute(Application.Driver.MakeAttribute(Color.Black, bg));
            foreach (var kf in sprite.Keyframes.Keys)
            {
                var idx = kf - 1;
                if (idx < start || idx > end)
                {
                    continue;
                }
                Move(LabelWidth + idx - offsetX, y);
                Driver.AddRune('o');
            }
            Driver.SetAttribute(ColorScheme.Normal);
        }

        var cursorX = LabelWidth + _cursorFrame - offsetX;
        var cursorY = _cursorChannel - offsetY + 1;
        if (cursorX >= LabelWidth && cursorX < w && cursorY >= 1 && cursorY < h)
        {
            Driver.SetAttribute(Application.Driver.MakeAttribute(Color.Black, Color.White));
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
            var menuItems = new List<string>
            {
                "Delete Sprite",
                "Add Keyframe",
                "Move Keyframe",
                "Change Start",
                "Change End",
                "Play From Here",
                "Select Sprite"
            };
            if (sprite.Keyframes.ContainsKey(_cursorFrame + 1))
            {
                menuItems.Insert(1, "Edit Keyframe");
            }
            items = menuItems.ToArray();
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
                if (_cursorChannel >= SpecialChannels.Length)
                {
                    CreateSpriteDialog();
                }
                break;
            case "Delete Sprite":
                if (sprite != null)
                {
                    _sprites.Remove(sprite);
                    SetNeedsDisplay();
                    NotifyInfoChanged();
                    NotifySpriteChanged();
                }
                break;
            case "Add Keyframe":
                if (sprite != null)
                {
                    var val = PromptForInt("Add Keyframe", "Frame:");
                    if (val.HasValue)
                    {
                        sprite.Keyframes[val.Value] = new HashSet<string>();
                        EditKeyframeDialog(sprite, val.Value);
                        SetNeedsDisplay();
                        NotifySpriteChanged();
                    }
                }
                break;
            case "Move Keyframe":
                if (sprite != null && sprite.Keyframes.TryGetValue(_cursorFrame + 1, out var props))
                {
                    var val = PromptForInt("Move Keyframe", "Frame:");
                    if (val.HasValue)
                    {
                        sprite.Keyframes.Remove(_cursorFrame + 1);
                        sprite.Keyframes[val.Value] = props;
                        _cursorFrame = val.Value - 1;
                        SetNeedsDisplay();
                        NotifyInfoChanged();
                        NotifySpriteChanged();
                    }
                }
                break;
            case "Edit Keyframe":
                if (sprite != null && sprite.Keyframes.ContainsKey(_cursorFrame + 1))
                {
                    EditKeyframeDialog(sprite, _cursorFrame + 1);
                    SetNeedsDisplay();
                    NotifySpriteChanged();
                }
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
                        NotifySpriteChanged();
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
                        NotifySpriteChanged();
                    }
                }
                break;
            case "Play From Here":
                PlayFromHere?.Invoke(_cursorFrame + 1);
                break;
            case "Select Sprite":
                if (sprite != null)
                {
                    SelectSprite(sprite.Number);
                    SpriteSelected?.Invoke(sprite.Number);
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
                var ch = _cursorChannel + 1 - SpecialChannels.Length;
                _sprites.Add(new SpriteBlock(ch, b, e, num, member.Text?.ToString() ?? string.Empty));
                SetNeedsDisplay();
                NotifyInfoChanged();
                NotifySpriteChanged();
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
        begin.SetFocus();
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
        field.SetFocus();
        Application.Run(dialog);
        return result;
    }

    private void EditKeyframeDialog(SpriteBlock sprite, int frame)
    {
        if (!sprite.Keyframes.TryGetValue(frame, out var props))
        {
            props = new HashSet<string>();
            sprite.Keyframes[frame] = props;
        }
        var checks = new List<CheckBox>();
        for (var i = 0; i < TweenProperties.Length; i++)
        {
            var name = TweenProperties[i];
            var cb = new CheckBox(name) { X = 1, Y = i + 1, Checked = props.Contains(name) };
            checks.Add(cb);
        }
        var ok = new Button("Ok", true);
        ok.Clicked += () =>
        {
            sprite.Keyframes[frame] = checks
                .Where(c => c.Checked)
                .Select(c => c.Text.ToString() ?? string.Empty)
                .ToHashSet();
            Application.RequestStop();
        };
        var dialog = new Dialog("Keyframe", 30, TweenProperties.Length + 4, ok);
        foreach (var cb in checks)
        {
            dialog.Add(cb);
        }
        Application.Run(dialog);
    }

    private SpriteBlock? FindSprite(int channel, int frame)
    {
        if (channel <= SpecialChannels.Length)
        {
            return null;
        }
        var spriteChannel = channel - SpecialChannels.Length;
        foreach (var sprite in _sprites)
        {
            if (sprite.Channel == spriteChannel && frame >= sprite.Start && frame <= sprite.End)
            {
                return sprite;
            }
        }
        return null;
    }

    private void NotifyInfoChanged()
    {
        var sprite = FindSprite(_cursorChannel + 1, _cursorFrame + 1);
        var ch = _cursorChannel + 1;
        if (ch > SpecialChannels.Length)
        {
            ch -= SpecialChannels.Length;
        }
        else
        {
            ch = 0;
        }
        InfoChanged?.Invoke(_cursorFrame + 1, ch, sprite?.Number, sprite?.MemberName);
    }

    public void TriggerInfo() => NotifyInfoChanged();

    private void NotifySpriteChanged() => SpriteChanged?.Invoke();

    public event Action<int>? SpriteSelected;

    public event Action<int>? PlayFromHere;

    public event Action? SpriteChanged;

    private sealed class SpriteBlock
    {
        public int Channel { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
        public int Number { get; }
        public string MemberName { get; }
        public Dictionary<int, HashSet<string>> Keyframes { get; } = new();

        public SpriteBlock(int channel, int start, int end, int number, string memberName)
        {
            Channel = channel;
            Start = start;
            End = end;
            Number = number;
            MemberName = memberName;
            Keyframes[start] = new HashSet<string>();
            Keyframes[end] = new HashSet<string>();
        }
    }
}
