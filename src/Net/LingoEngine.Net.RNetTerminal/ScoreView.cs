using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LingoEngine.IO.Data.DTO;
using Terminal.Gui;

namespace LingoEngine.Net.RNetTerminal;

internal sealed class ScoreView : ScrollView
{
    private const int SpriteChannelCount = 100;
    private const int FrameCount = 600;
    private const int StageWidth = 640;
    private readonly int _labelWidth;
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
    private readonly Dictionary<int, LingoMemberDTO> _members;
    private int? _selectedSprite;

    private int TotalChannels => SpriteChannelCount + SpecialChannels.Length;

    public ScoreView()
    {
        _sprites = TestMovieBuilder.BuildSprites()
            .Select(s => new SpriteBlock(s.SpriteNum, s.BeginFrame, s.EndFrame, s.SpriteNum, s.MemberNum, s.Name, s.Width))
            .ToList();
        _members = TestCastBuilder.BuildCastData()
            .SelectMany(c => c.Value)
            .ToDictionary(MemberKey);
        _labelWidth = Math.Max(SpecialChannels.Max(s => s.Length), SpriteChannelCount.ToString().Length) + 1;
        CanFocus = true;
        ColorScheme = new ColorScheme
        {
            Normal = Application.Driver.MakeAttribute(Color.White, Color.DarkGray)
        };
        ContentSize = new Size(FrameCount + _labelWidth, TotalChannels + 1);
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
            var frame = ContentOffset.X + me.X - _labelWidth;
            var channel = ContentOffset.Y + me.Y - 1;
            if (frame >= 0 && frame < FrameCount && channel >= 0 && channel < TotalChannels)
            {
                _cursorFrame = frame;
                _cursorChannel = channel;
                EnsureVisible();
                SetNeedsDisplay();
                NotifyInfoChanged();
                SetFocus();
            }
            ClampContentOffset();
            return true;
        }
        if (me.Flags.HasFlag(MouseFlags.Button3Clicked))
        {
            var frame = ContentOffset.X + me.X - _labelWidth;
            var channel = ContentOffset.Y + me.Y - 1;
            if (frame >= 0 && frame < FrameCount && channel >= 0 && channel < TotalChannels)
            {
                _cursorFrame = frame;
                _cursorChannel = channel;
                EnsureVisible();
                SetNeedsDisplay();
                NotifyInfoChanged();
                SetFocus();
                ShowActionMenu();
            }
            ClampContentOffset();
            return true;
        }
        if (me.Flags.HasFlag(MouseFlags.WheeledUp))
        {
            ContentOffset = new Point(ContentOffset.X, ContentOffset.Y - 1);
            ClampContentOffset();
            SetNeedsDisplay();
            return true;
        }
        if (me.Flags.HasFlag(MouseFlags.WheeledDown))
        {
            ContentOffset = new Point(ContentOffset.X, ContentOffset.Y + 1);
            ClampContentOffset();
            SetNeedsDisplay();
            return true;
        }
        if (me.Flags.HasFlag(MouseFlags.WheeledLeft))
        {
            ContentOffset = new Point(ContentOffset.X - 1, ContentOffset.Y);
            ClampContentOffset();
            SetNeedsDisplay();
            return true;
        }
        if (me.Flags.HasFlag(MouseFlags.WheeledRight))
        {
            ContentOffset = new Point(ContentOffset.X + 1, ContentOffset.Y);
            ClampContentOffset();
            SetNeedsDisplay();
            return true;
        }
        var handled = base.MouseEvent(me);
        ClampContentOffset();
        return handled;
    }

    private void ClampContentOffset()
    {
        var offset = ContentOffset;
        var scrollBarWidth = ShowVerticalScrollIndicator ? 1 : 0;
        var scrollBarHeight = ShowHorizontalScrollIndicator ? 1 : 0;
        var visibleFrames = Math.Max(0, Bounds.Width - _labelWidth - scrollBarWidth);
        var visibleChannels = Math.Max(0, Bounds.Height - 1 - scrollBarHeight);
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
        var scrollBarWidth = ShowVerticalScrollIndicator ? 1 : 0;
        var scrollBarHeight = ShowHorizontalScrollIndicator ? 1 : 0;
        var visibleFrames = Math.Max(0, Bounds.Width - _labelWidth - scrollBarWidth);
        var visibleChannels = Math.Max(0, Bounds.Height - 1 - scrollBarHeight);
        var offset = ContentOffset;
        if (_cursorFrame < offset.X)
        {
            offset.X = _cursorFrame;
        }
        else if (_cursorFrame >= offset.X + visibleFrames - 1)
        {
            offset.X = _cursorFrame - visibleFrames + 1;
        }
        if (_cursorChannel < offset.Y)
        {
            offset.Y = _cursorChannel;
        }
        else if (_cursorChannel >= offset.Y + visibleChannels - 1)
        {
            offset.Y = _cursorChannel - visibleChannels + 1;
        }
        ContentOffset = offset;
        ClampContentOffset();
    }

    public override void Redraw(Rect bounds)
    {
        base.Redraw(bounds);
        var scrollBarWidth = ShowVerticalScrollIndicator ? 1 : 0;
        var scrollBarHeight = ShowHorizontalScrollIndicator ? 1 : 0;
        var w = Bounds.Width - scrollBarWidth;
        var h = Bounds.Height - scrollBarHeight;
        Driver.SetAttribute(ColorScheme.Normal);
        for (var y = 0; y < h; y++)
        {
            Move(0, y);
            for (var x = 0; x < w; x++)
            {
                Driver.AddRune(' ');
            }
        }

        var visibleFrames = Math.Max(0, w - _labelWidth);
        var visibleChannels = Math.Max(0, h - 1);
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
            label = label.PadLeft(_labelWidth - 1);
            Driver.AddStr(label + "|");
        }

        for (var f = 0; f < visibleFrames && offsetX + f < FrameCount; f++)
        {
            var frame = offsetX + f + 1;
            if (frame % 10 == 0)
            {
                var label = frame.ToString();
                var pos = _labelWidth + f - label.Length + 1;
                if (pos >= _labelWidth)
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
            var pos = _labelWidth + _playFrame - offsetX - label.Length + 1;
            if (pos >= _labelWidth)
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
                Move(_labelWidth + f - offsetX, y);
                Driver.AddRune(' ');
            }
            if (_members.TryGetValue(sprite.MemberNum, out var member) && member.Type == LingoMemberTypeDTO.Text)
            {
                var maxChars = Math.Max(1, (int)(sprite.Width / StageWidth * (end - start + 1)));
                var text = member.Name;
                var len = Math.Min(text.Length, Math.Min(maxChars, end - start + 1));
                Driver.SetAttribute(Application.Driver.MakeAttribute(Color.Black, bg));
                for (var i = 0; i < len; i++)
                {
                    Move(_labelWidth + start + i - offsetX, y);
                    Driver.AddRune(text[i]);
                }
            }
            Driver.SetAttribute(Application.Driver.MakeAttribute(Color.Black, bg));
            foreach (var kf in sprite.Keyframes.Keys)
            {
                var idx = kf - 1;
                if (idx < start || idx > end)
                {
                    continue;
                }
                Move(_labelWidth + idx - offsetX, y);
                Driver.AddRune('o');
            }
            Driver.SetAttribute(ColorScheme.Normal);
        }

        var cursorX = _labelWidth + _cursorFrame - offsetX;
        var cursorY = _cursorChannel - offsetY + 1;
        if (cursorX >= _labelWidth && cursorX < w && cursorY >= 1 && cursorY < h)
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
                        sprite.Keyframes[val.Value] = new Dictionary<string, double>();
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
                var name = member.Text?.ToString() ?? string.Empty;
                var memberEntry = _members.Values.FirstOrDefault(m => m.Name == name);
                var memberNum = memberEntry?.Number ?? 0;
                var width = memberEntry?.Width ?? 1;
                _sprites.Add(new SpriteBlock(ch, b, e, num, memberNum, name, width));
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
            props = new Dictionary<string, double>();
            sprite.Keyframes[frame] = props;
        }
        var rows = new List<(CheckBox cb, TextField field, string name)>();
        for (var i = 0; i < TweenProperties.Length; i++)
        {
            var name = TweenProperties[i];
            var cb = new CheckBox(name) { X = 1, Y = i + 1, Checked = props.ContainsKey(name) };
            var field = new TextField(props.TryGetValue(name, out var val) ? val.ToString(CultureInfo.InvariantCulture) : "0")
            {
                X = 15,
                Y = i + 1,
                Width = 10,
                Visible = cb.Checked
            };
            cb.Toggled += _ => field.Visible = cb.Checked;
            rows.Add((cb, field, name));
        }
        var ok = new Button("Ok", true);
        ok.Clicked += () =>
        {
            var result = new Dictionary<string, double>();
            foreach (var (cb, field, name) in rows)
            {
                if (cb.Checked && double.TryParse(field.Text.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var val))
                {
                    result[name] = val;
                }
            }
            sprite.Keyframes[frame] = result;
            Application.RequestStop();
        };
        var dialog = new Dialog("Keyframe", 40, TweenProperties.Length + 4, ok);
        foreach (var (cb, field, _) in rows)
        {
            dialog.Add(cb, field);
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

    private static int MemberKey(LingoMemberDTO member)
        => (member.CastLibNum << 16) | member.NumberInCast;

    private sealed class SpriteBlock
    {
        public int Channel { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
        public int Number { get; }
        public int MemberNum { get; }
        public string MemberName { get; }
        public float Width { get; }
        public Dictionary<int, Dictionary<string, double>> Keyframes { get; } = new();

        public SpriteBlock(int channel, int start, int end, int number, int memberNum, string memberName, float width)
        {
            Channel = channel;
            Start = start;
            End = end;
            Number = number;
            MemberNum = memberNum;
            MemberName = memberName;
            Width = width;
            Keyframes[start] = new Dictionary<string, double>();
            Keyframes[end] = new Dictionary<string, double>();
        }
    }
}
