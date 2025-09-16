using LingoEngine.IO.Data.DTO.Members;
using LingoEngine.IO.Data.DTO.Sprites;
using LingoEngine.Net.RNetTerminal.Datas;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Terminal.Gui;
using Terminal.Gui.App;
using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace LingoEngine.Net.RNetTerminal.Views;

internal sealed class ScoreView : View
{
   
    private int FrameCount => TerminalDataStore.Instance.FrameCount;
    private int _stageWidth;
    private readonly int _labelWidth;
    private static readonly string[] SpecialChannels =
    {
        "Tempo",
        "Transition",
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
    private readonly List<SpecialSpriteBlock>[] _specialChannelSprites;
    private SpriteRef? _selectedSprite;
    private SpriteBlock? _dragSprite;
    private SpriteBlock? _dragCandidate;
    private bool _isDragging;
    private int _dragOffset;
    private int _dragOriginalStart;
    private int _dragLength;

    public Rectangle Bounds => Viewport;
    public Point ContentOffset {
        get => new Point(HorizontalScrollBar.Position, VerticalScrollBar.Position);
        set => (HorizontalScrollBar.Position, VerticalScrollBar.Position) = (value.X, value.Y);
    }
    private int TotalChannels => TerminalDataStore.Instance.SpriteChannelCount + SpecialChannels.Length;

    public ScoreView()
    {
        _sprites = new List<SpriteBlock>();
        _specialChannelSprites = Enumerable.Range(0, SpecialChannels.Length)
            .Select(_ => new List<SpecialSpriteBlock>())
            .ToArray();
        _labelWidth = System.Math.Max(SpecialChannels.Max(s => s.Length), TerminalDataStore.Instance.SpriteChannelCount.ToString().Length) + 1;
        CanFocus = true;
        WantMousePositionReports = true;
        SetScheme(new Scheme
        {
            Normal = new Attribute(ColorName16.White, ColorName16.DarkGray)
        });
        //ContentSize = new Size(FrameCount + _labelWidth, TotalChannels + 1);
        Width = FrameCount + _labelWidth;
        Height = TotalChannels + 1;
        HorizontalScrollBar.Visible = true;
        VerticalScrollBar.Visible = true;
        var store = TerminalDataStore.Instance;
        _selectedSprite = store.GetSelectedSprite();
        store.SelectedSpriteChanged += s =>
        {
            _selectedSprite = s;
            SetNeedsDraw();
        };
        store.SpritesChanged += ReloadData;
        store.CastsChanged += ReloadData;
        store.SpriteChanged += _ => SetNeedsDraw();
        store.MemberChanged += _ => SetNeedsDraw();
        ReloadData();
    }

    public event System.Action<int, int, SpriteRef?, MemberRef?>? InfoChanged;

    public void RequestRedraw() => SetNeedsDraw();
    public void ReloadData()
    {
        var store = TerminalDataStore.Instance;
        _stageWidth = store.StageWidth;
        _sprites.Clear();
        _sprites.AddRange(store.GetSprites()
            .Select(s => new SpriteBlock(s.SpriteNum, s.BeginFrame, s.EndFrame, s.SpriteNum, s.Member!.CastLibNum, s.Member.MemberNum, s.Width)));
        foreach (var list in _specialChannelSprites)
        {
            list.Clear();
        }

        foreach (var tempo in store.GetTempoSprites())
        {
            AddSpecialSprite(0, tempo.BeginFrame, tempo.EndFrame, FormatTempoLabel(tempo), ColorName16.BrightYellow);
        }

        foreach (var transition in store.GetTransitionSprites())
        {
            var label = string.IsNullOrWhiteSpace(transition.Name)
                ? transition.Settings?.TransitionName ?? "Transition"
                : transition.Name;
            AddSpecialSprite(1, transition.BeginFrame, transition.EndFrame, label ?? string.Empty, ColorName16.Magenta);
        }

        foreach (var palette in store.GetColorPaletteSprites())
        {
            var label = string.IsNullOrWhiteSpace(palette.Name)
                ? "Palette"
                : palette.Name;
            AddSpecialSprite(2, palette.BeginFrame, palette.EndFrame, label, ColorName16.BrightCyan);
        }

        foreach (var sound in store.GetSoundSprites())
        {
            var idx = sound.Channel switch
            {
                1 => 4,
                2 => 5,
                _ => -1
            };

            if (idx < 0 || idx >= _specialChannelSprites.Length)
            {
                continue;
            }

            var label = sound.Member != null
                ? store.FindMember(sound.Member.CastLibNum, sound.Member.MemberNum)?.Name ?? sound.Name
                : sound.Name;
            AddSpecialSprite(idx, sound.BeginFrame, sound.EndFrame, label ?? string.Empty, ColorName16.Green);
        }
        //ContentSize = new Size(FrameCount + _labelWidth, TotalChannels + 1);
        Width = FrameCount + _labelWidth;
        Height = TotalChannels + 1;
        SetNeedsDraw();
    }

    private static string FormatTempoLabel(LingoTempoSpriteDTO tempo)
    {
        return tempo.Action switch
        {
            LingoTempoSpriteActionDTO.ChangeTempo => $"Tempo:{tempo.Tempo}",
            LingoTempoSpriteActionDTO.WaitSeconds => $"Wait:{tempo.WaitSeconds.ToString("0.##", CultureInfo.InvariantCulture)}",
            LingoTempoSpriteActionDTO.WaitForUserInput => "WaitInput",
            LingoTempoSpriteActionDTO.WaitForCuePoint => $"Cue:{tempo.CueChannel}:{tempo.CuePoint}",
            _ => tempo.Name
        };
    }

    private void AddSpecialSprite(int channelIndex, int beginFrame, int endFrame, string label, ColorName16 background)
    {
        if (channelIndex < 0 || channelIndex >= _specialChannelSprites.Length)
        {
            return;
        }

        var start = beginFrame <= 0 ? endFrame : beginFrame;
        if (start <= 0)
        {
            start = 1;
        }
        var finish = endFrame <= 0 ? start : endFrame;
        if (finish < start)
        {
            finish = start;
        }
        start = System.Math.Clamp(start, 1, FrameCount > 0 ? FrameCount : start);
        finish = System.Math.Clamp(finish, start, FrameCount > 0 ? FrameCount : finish);

        _specialChannelSprites[channelIndex].Add(new SpecialSpriteBlock(start, finish, label, background));
    }
    protected override bool OnKeyDown(Key key)
    {
        try
        {
            if (!HasFocus) return false;
            var step = 1;
            var ctrl = key.IsCtrl;
            var shift = key.IsShift;
            var keyNoMods = ctrl == false && shift == false;
            if (ctrl && shift)
            {
                step = 20;
            }
            else if (ctrl)
            {
                step = 10;
            }
            switch (key.KeyCode)
            {
                case Terminal.Gui.Drivers.KeyCode.CursorUp:
                    MoveCursor(0, -step);
                    return true;
                case Terminal.Gui.Drivers.KeyCode.CursorDown:
                    MoveCursor(0, step);
                    return true;
                case Terminal.Gui.Drivers.KeyCode.CursorLeft:
                    MoveCursor(-step, 0);
                    return true;
                case Terminal.Gui.Drivers.KeyCode.CursorRight:
                    MoveCursor(step, 0);
                    return true;
                case Terminal.Gui.Drivers.KeyCode.Enter:
                    ShowActionMenu();
                    return true;
            }
            return base.OnKeyDown(key);
        }
        catch (System.Exception ex)
        {
            System.Console.Error.WriteLine($"ProcessKey error: {ex}");
            return false;
        }
    }

    protected override bool OnMouseEvent(MouseEventArgs me)
    {
        try
        {
            if (!HasFocus) return false;
            var Bounds = Viewport;
            var scrollBarWidth = VerticalScrollBar.Visible ? 1 : 0;
            var scrollBarHeight = VerticalScrollBar.Visible ? 1 : 0;
            var contentW = Bounds.Width - scrollBarWidth;
            var contentH = Bounds.Height - scrollBarHeight;
            var inContent = me.Position.X < contentW && me.Position.Y < contentH;
            var offset = GetOffset();
            var frame = -1;
            var channel = -1;
            SpriteBlock? sprite = null;

            if (inContent)
            {
                frame = offset.X + me.Position.X - _labelWidth;
                channel = offset.Y + me.Position.Y - 1;
                if (frame >= 0 && frame < FrameCount && channel >= 0 && channel < TotalChannels)
                {
                    sprite = FindSprite(channel + 1, frame + 1);
                }

                if (me.Flags.HasFlag(MouseFlags.Button1Pressed))
                {
                    if (sprite != null)
                    {
                        PrepareDrag(sprite, frame);
                    }
                    else
                    {
                        _dragCandidate = null;
                    }
                }

                if (me.Flags.HasFlag(MouseFlags.Button1DoubleClicked) && sprite != null)
                {
                    var sel = new SpriteRef(sprite.Number, sprite.Start);
                    TerminalDataStore.Instance.SelectSprite(sel);
                }

                if (_dragCandidate != null && me.Flags.HasFlag(MouseFlags.Button1Pressed) && me.Flags.HasFlag(MouseFlags.ReportMousePosition))
                {
                    UpdateDrag(frame);
                    return true;
                }

                if (me.Flags.HasFlag(MouseFlags.Button1Released))
                {
                    if (_isDragging)
                    {
                        FinishDrag();
                        ClampContentOffset();
                        return true;
                    }

                    _dragCandidate = null;
                }
            }

            if (inContent && me.Flags.HasFlag(MouseFlags.Button1Clicked))
            {
                if (frame >= 0 && frame < FrameCount && channel >= 0 && channel < TotalChannels)
                {
                    _cursorFrame = frame;
                    _cursorChannel = channel;
                    EnsureVisible();
                    SetNeedsDraw();
                    NotifyInfoChanged();
                    SetFocus();
                    if (sprite != null)
                    {
                        var sel = new SpriteRef(sprite.Number, sprite.Start);
                        TerminalDataStore.Instance.SelectSprite(sel);
                    }
                }
                ClampContentOffset();
                return true;
            }
            if (inContent && me.Flags.HasFlag(MouseFlags.Button3Clicked))
            {
                if (frame >= 0 && frame < FrameCount && channel >= 0 && channel < TotalChannels)
                {
                    _cursorFrame = frame;
                    _cursorChannel = channel;
                    EnsureVisible();
                    SetNeedsDraw();
                    NotifyInfoChanged();
                    SetFocus();
                    ShowActionMenu();
                }
                ClampContentOffset();
                return true;
            }
            if (me.Flags.HasFlag(MouseFlags.WheeledUp))
            {
                offset = GetOffset();
                offset.Y--;
                ClampContentOffset(ref offset);
                SetOffset(offset);
                SetNeedsDraw();
                return true;
            }
            if (me.Flags.HasFlag(MouseFlags.WheeledDown))
            {
                offset = GetOffset();
                offset.Y++;
                ClampContentOffset(ref offset);
                SetOffset(offset);
                SetNeedsDraw();
                return true;
            }
            if (me.Flags.HasFlag(MouseFlags.WheeledLeft))
            {
                offset = GetOffset();
                offset.X--;
                ClampContentOffset(ref offset);
                SetOffset(offset);
                SetNeedsDraw();
                return true;
            }
            if (me.Flags.HasFlag(MouseFlags.WheeledRight))
            {
                offset = GetOffset();
                offset.X++;
                ClampContentOffset(ref offset);
                SetOffset(offset);
                SetNeedsDraw();
                return true;
            }
            var handled = base.OnMouseEvent(me);
            ClampContentOffset();
            SetNeedsDraw();
            return handled;
        }
        catch (System.Exception ex)
        {
            System.Console.Error.WriteLine($"MouseEvent error: {ex}");
            return false;
        }
    }
    
    private void ClampContentOffset(ref Point offset)
    {
       
        var scrollBarWidth = VerticalScrollBar.Visible ? 1 : 0;
        var scrollBarHeight = VerticalScrollBar.Visible ? 1 : 0;
        var visibleFrames = System.Math.Max(0, Bounds.Width - _labelWidth - scrollBarWidth);
        var visibleChannels = System.Math.Max(0, Bounds.Height - 1 - scrollBarHeight);
        var maxX = System.Math.Max(0, FrameCount - visibleFrames);
        var maxY = System.Math.Max(0, TotalChannels - visibleChannels);
        offset.X = System.Math.Clamp(offset.X, 0, maxX);
        offset.Y = System.Math.Clamp(offset.Y, 0, maxY);
    }

    private void ClampContentOffset()
    {
        var offset = GetOffset();
        ClampContentOffset(ref offset);
        SetOffset(offset);
    }

    private Point GetOffset() => new(-ContentOffset.X, -ContentOffset.Y);

    private void SetOffset(Point offset) => ContentOffset = new Point(-offset.X, -offset.Y);

    private void MoveCursor(int dx, int dy)
    {
        _cursorFrame = System.Math.Clamp(_cursorFrame + dx, 0, FrameCount - 1);
        _cursorChannel = System.Math.Clamp(_cursorChannel + dy, 0, TotalChannels - 1);
        var sprite = FindSprite(_cursorChannel + 1, _cursorFrame + 1);
        EnsureVisible();
        SetNeedsDraw();
        NotifyInfoChanged();
        if (sprite != null)
        {
            var sel = new SpriteRef(sprite.Number, sprite.Start);
            TerminalDataStore.Instance.SelectSprite(sel);
        }
    }

    private void EnsureVisible()
    {
        var scrollBarWidth = VerticalScrollBar.Visible ? 1 : 0;
        var scrollBarHeight = VerticalScrollBar.Visible ? 1 : 0;
        var visibleFrames = System.Math.Max(0, Bounds.Width - _labelWidth - scrollBarWidth);
        var visibleChannels = System.Math.Max(0, Bounds.Height - 1 - scrollBarHeight);
        var offset = GetOffset();
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
        ClampContentOffset(ref offset);
        SetOffset(offset);
    }

    //public override void Redraw(Rect bounds)
    protected override bool OnDrawingContent(DrawContext? context)
    {
        
        ClampContentOffset();
        //base.Redraw(bounds);
        var bounds = Viewport;
        //BackgroundHelper.ClearWith(bounds, 'a', new Terminal.Gui.Attribute(Color.Red));
        var scrollBarWidth = VerticalScrollBar.Visible ? 1 : 0;
        var scrollBarHeight = VerticalScrollBar.Visible ? 1 : 0;
        var w = Bounds.Width - scrollBarWidth;
        var h = Bounds.Height - scrollBarHeight;
        //SetAttribute(new Terminal.Gui.Attribute(Color.Black,Color.White));
        SetColorsSchemaNormal();
        var y = 0;
        // Draw top bar
        Move(0, y);
        for (var x = 0; x < w; x++)
            AddRune(' ');
        // Draw background
        SetAttribute(new Attribute(ColorName16.DarkGray, ColorName16.Black));
        for (y = 1; y < h; y++)
        {
            Move(0, y);
            for (var x = 0; x < w; x++)
                AddRune(RNetTerminalStyle.CharLight);
        }


        SetColorsSchemaNormal();
        var visibleFrames = System.Math.Max(0, w - _labelWidth);
        var visibleChannels = System.Math.Max(0, h - 1);
        var posOffset = GetOffset();
        var offsetX = posOffset.X;
        var offsetY = posOffset.Y;

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
            AddStr(label + "|");
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
                    AddStr(label);
                }
            }
        }

        DrawSpecialSprites(offsetX, offsetY, visibleFrames, visibleChannels);

        if (_playFrame >= offsetX && _playFrame < offsetX + visibleFrames)
        {
            var frame = _playFrame + 1;
            var label = frame.ToString();
            var pos = _labelWidth + _playFrame - offsetX - label.Length + 1;
            if (pos >= _labelWidth)
            {
                SetAttribute(new Attribute(ColorName16.BrightRed, ColorName16.BrightBlue));
                Move(pos, 0);
                AddStr(label);
                SetColorsSchemaNormal();
            }
        }

        foreach (var sprite in _sprites)
        {
            var channelIdx = sprite.Channel - 1 + SpecialChannels.Length;
            if (channelIdx < offsetY || channelIdx >= offsetY + visibleChannels)
            {
                continue;
            }
            var start = System.Math.Max(sprite.Start - 1, offsetX);
            var end = System.Math.Min(sprite.End - 1, offsetX + visibleFrames - 1);
            if (end < offsetX || start > offsetX + visibleFrames - 1)
            {
                continue;
            }
            y = channelIdx - offsetY + 1;
            var bg = _selectedSprite.HasValue && sprite.Number == _selectedSprite.Value.SpriteNum && sprite.Start == _selectedSprite.Value.BeginFrame ? ColorName16.Blue : ColorName16.BrightBlue;
            SetAttribute(new Attribute(ColorName16.White, bg));
            for (var f = start; f <= end; f++)
            {
                Move(_labelWidth + f - offsetX, y);
                AddRune(' ');
            }
            var member = TerminalDataStore.Instance.FindMember(sprite.CastLib, sprite.MemberNum);
            if (member != null && member.Type == LingoMemberTypeDTO.Text)
            {
                var maxChars = System.Math.Max(1, (int)(sprite.Width / _stageWidth * (end - start + 1)));
                var text = member.Name;
                var len = System.Math.Min(text.Length, System.Math.Min(maxChars, end - start + 1));
                SetAttribute(new Attribute(ColorName16.Black, bg));
                for (var i = 0; i < len; i++)
                {
                    Move(_labelWidth + start + i - offsetX, y);
                    AddRune(text[i]);
                }
            }
            SetAttribute(new Attribute(ColorName16.Black, bg));
            foreach (var kf in sprite.Keyframes.Keys)
            {
                var idx = kf - 1;
                if (idx < start || idx > end)
                {
                    continue;
                }
                Move(_labelWidth + idx - offsetX, y);
                AddRune('o');
            }
            SetColorsSchemaNormal();
        }

        var cursorX = _labelWidth + _cursorFrame - offsetX;
        var cursorY = _cursorChannel - offsetY + 1;
        if (cursorX >= _labelWidth && cursorX < w && cursorY >= 1 && cursorY < h)
        {
            SetAttribute(new Attribute(ColorName16.Black, ColorName16.Green));
            Move(cursorX, cursorY);
            AddRune(' ');
            SetColorsSchemaNormal();
        }
        return base.OnDrawingContent(context);
    }
    private void SetColorsSchemaNormal()
    {
        //SetAttribute(ColorScheme.Normal);
        SetScheme(RNetTerminalStyle.DefaultScheme);
    }

    public void SetPlayFrame(int frame)
    {
        _playFrame = System.Math.Clamp(frame, 0, FrameCount - 1);
        SetNeedsDraw();
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

        var list = new ListView()
        {
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        list.SetSource(new System.Collections.ObjectModel.ObservableCollection<string>(items));
        var dialog = RUI.NewDialog("Score", 30, items.Length + 4);
        dialog.Add(list);
        list.OpenSelectedItem += (_,args) =>
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
                    SetNeedsDraw();
                    NotifyInfoChanged();
                }
                break;
            case "Add Keyframe":
                if (sprite != null)
                {
                    var frame = _cursorFrame + 1;
                    sprite.Keyframes[frame] = new Dictionary<string, double>();
                    EditKeyframeDialog(sprite, frame);
                    SetNeedsDraw();
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
                        SetNeedsDraw();
                        NotifyInfoChanged();
                    }
                }
                break;
            case "Edit Keyframe":
                if (sprite != null && sprite.Keyframes.ContainsKey(_cursorFrame + 1))
                {
                    EditKeyframeDialog(sprite, _cursorFrame + 1);
                    SetNeedsDraw();
                }
                break;
            case "Change Start":
                if (sprite != null)
                {
                    var val = PromptForInt("Change Start", "Start:");
                    if (val.HasValue)
                    {
                        sprite.Start = val.Value;
                        SetNeedsDraw();
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
                        SetNeedsDraw();
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
                    var sel = new SpriteRef(sprite.Number, sprite.Start);
                    TerminalDataStore.Instance.SelectSprite(sel);
                    NotifyInfoChanged();
                }
                break;
        }
    }

    private void CreateSpriteDialog()
    {
        var begin = new TextField{ Text = "1", X = 14, Y = 1, Width = 10 };
        var end = new TextField{ Text = "1", X = 14, Y = 3, Width = 10 };
        var locH = new TextField{ Text = "0", X = 14, Y = 5, Width = 10 };
        var locV = new TextField{ Text = "0", X = 14, Y = 7, Width = 10 };
        var castLib = new TextField{ Text = "1", X = 14, Y = 9, Width = 10 };
        var memberNum = new TextField{ Text = "1", X = 14, Y = 11, Width = 10 };
        var ok = RUI.NewButton("Ok", true,() =>
        {
            if (int.TryParse(begin.Text.ToString(), out var b) &&
                int.TryParse(end.Text.ToString(), out var e) &&
                int.TryParse(castLib.Text.ToString(), out var cast) &&
                int.TryParse(memberNum.Text.ToString(), out var mem))
            {
                var num = _sprites.Count + 1;
                var ch = _cursorChannel + 1 - SpecialChannels.Length;
                var memberEntry = TerminalDataStore.Instance.FindMember(cast, mem);
                var width = memberEntry?.Width ?? 1;
                _sprites.Add(new SpriteBlock(ch, b, e, num, cast, mem, width));
                SetNeedsDraw();
                NotifyInfoChanged();
            }
            Application.RequestStop();
        });
        var dialog = RUI.NewDialog("Create Sprite", 40, 17, ok);
        dialog.Add(
            new Label{ Text = "Begin:", X = 1, Y = 1 }, begin,
            new Label{ Text = "End:", X = 1, Y = 3 }, end,
            new Label{ Text = "LocH:", X = 1, Y = 5 }, locH,
            new Label{ Text = "LocV:", X = 1, Y = 7 }, locV,
            new Label{ Text = "CastLibNum:", X = 1, Y = 9 }, castLib,
            new Label{ Text = "MemberNum:", X = 1, Y = 11 }, memberNum);
        begin.SetFocus();
        Application.Run(dialog);
    }

    private int? PromptForInt(string title, string prompt)
    {
        int? result = null;
        var field = new TextField(){Text = "0", X = 12, Y = 1, Width = 10 };
        var ok = RUI.NewButton("Ok", true,() =>
        {
            if (int.TryParse(field.Text.ToString(), out var v))
            {
                result = v;
            }
            Application.RequestStop();
        });
        var dialog = RUI.NewDialog(title, 30, 7, ok);
        dialog.Add(new Label{ Text = prompt, X = 1, Y = 1 }, field);
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
            var cb = new CheckBox() { Text = name, X = 1, Y = i + 1, CheckedState = props.ContainsKey(name).ToCheckedSTate() };
            var field = new TextField()
            {
                Text = props.TryGetValue(name, out var val) ? val.ToString(CultureInfo.InvariantCulture) : "0",
                X = 15,
                Y = i + 1,
                Width = 10,
                Visible = cb.CheckedState.ToBool()
            };
            
            cb.CheckedStateChanged += (_,_) =>
            {
                field.Visible = cb.CheckedState.ToBool();
                if (cb.CheckedState.ToBool())
                {
                    field.SetFocus();
                }
            };
            rows.Add((cb, field, name));
        }
        var ok = RUI.NewButton("Ok", true,() =>
        {
            var result = new Dictionary<string, double>();
            foreach (var (cb, field, name) in rows)
            {
                if (cb.CheckedState.ToBool() && double.TryParse(field.Text.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var val))
                {
                    result[name] = val;
                }
            }
            sprite.Keyframes[frame] = result;
            Application.RequestStop();
        });
        var dialog = RUI.NewDialog("Keyframe", 40, TweenProperties.Length + 4, ok);
        foreach (var (cb, field, _) in rows)
        {
            dialog.Add(cb, field);
        }
        dialog.Add(RUI.NewLabel("Use Space to toggle",1, TweenProperties.Length + 1 ));
        rows[0].cb.SetFocus();
        Application.Run(dialog);
    }

    private void PrepareDrag(SpriteBlock sprite, int frame)
    {
        _dragCandidate = sprite;
        _dragSprite = sprite;
        _dragOriginalStart = sprite.Start;
        _dragLength = System.Math.Max(0, sprite.End - sprite.Start);
        _dragOffset = frame + 1 - sprite.Start;
        if (_dragOffset < 0)
        {
            _dragOffset = 0;
        }
    }

    private void UpdateDrag(int frame)
    {
        if (_dragSprite == null)
        {
            return;
        }

        if (!_isDragging)
        {
            _isDragging = true;
        }

        var maxStart = System.Math.Max(1, FrameCount - _dragLength);
        var newStart = System.Math.Clamp(frame + 1 - _dragOffset, 1, maxStart);
        var newEnd = newStart + _dragLength;
        if (newEnd > FrameCount)
        {
            newEnd = FrameCount;
            newStart = System.Math.Max(1, newEnd - _dragLength);
        }

        _dragSprite.Start = newStart;
        _dragSprite.End = newEnd;
        _cursorFrame = System.Math.Clamp(newStart - 1, 0, FrameCount - 1);
        _cursorChannel = _dragSprite.Channel - 1 + SpecialChannels.Length;
        EnsureVisible();
        SetNeedsDraw();
        NotifyInfoChanged();
    }

    private void FinishDrag()
    {
        if (_dragSprite == null)
        {
            _dragCandidate = null;
            _isDragging = false;
            return;
        }

        var delta = _dragSprite.Start - _dragOriginalStart;
        var store = TerminalDataStore.Instance;
        if (delta != 0)
        {
            store.MoveSprite(new SpriteRef(_dragSprite.Number, _dragOriginalStart), delta);
        }
        store.SelectSprite(new SpriteRef(_dragSprite.Number, _dragSprite.Start));
        _dragCandidate = null;
        _dragSprite = null;
        _isDragging = false;
    }

    private void DrawSpecialSprites(int offsetX, int offsetY, int visibleFrames, int visibleChannels)
    {
        for (var channelIndex = 0; channelIndex < _specialChannelSprites.Length; channelIndex++)
        {
            if (channelIndex < offsetY || channelIndex >= offsetY + visibleChannels)
            {
                continue;
            }

            var y = channelIndex - offsetY + 1;
            foreach (var block in _specialChannelSprites[channelIndex])
            {
                var start = System.Math.Max(block.Start - 1, offsetX);
                var end = System.Math.Min(block.End - 1, offsetX + visibleFrames - 1);
                if (end < offsetX || start > offsetX + visibleFrames - 1)
                {
                    continue;
                }

                var attr = new Attribute(ColorName16.Black, block.Background);
                SetAttribute(attr);
                for (var frame = start; frame <= end; frame++)
                {
                    Move(_labelWidth + frame - offsetX, y);
                    AddRune(' ');
                }

                var label = block.Label ?? string.Empty;
                var maxLen = System.Math.Max(0, end - start + 1);
                var len = System.Math.Min(label.Length, maxLen);
                for (var i = 0; i < len; i++)
                {
                    Move(_labelWidth + start + i - offsetX, y);
                    AddRune(label[i]);
                }

                SetColorsSchemaNormal();
            }
        }
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
        SpriteRef? spriteRef = null;
        MemberRef? memberRef = null;
        if (sprite != null)
        {
            spriteRef = new SpriteRef(sprite.Number, sprite.Start);
            memberRef = new MemberRef(sprite.CastLib, sprite.MemberNum);
        }
        InfoChanged?.Invoke(_cursorFrame + 1, ch, spriteRef, memberRef);
    }

    public void TriggerInfo() => NotifyInfoChanged();

    public event System.Action<int>? PlayFromHere;

    private sealed class SpecialSpriteBlock
    {
        public int Start { get; }
        public int End { get; }
        public string Label { get; }
        public ColorName16 Background { get; }

        public SpecialSpriteBlock(int start, int end, string label, ColorName16 background)
        {
            Start = start;
            End = end;
            Label = label;
            Background = background;
        }
    }

    private sealed class SpriteBlock
    {
        public int Channel { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
        public int Number { get; }
        public int CastLib { get; }
        public int MemberNum { get; }
        public float Width { get; }
        public Dictionary<int, Dictionary<string, double>> Keyframes { get; } = new();
        public SpriteBlock(int channel, int start, int end, int number, int castLib, int memberNum, float width)
        {
            Channel = channel;
            Start = start;
            End = end;
            Number = number;
            CastLib = castLib;
            MemberNum = memberNum;
            Width = width;
            Keyframes[start] = new Dictionary<string, double>();
            Keyframes[end] = new Dictionary<string, double>();
        }
    }
}
