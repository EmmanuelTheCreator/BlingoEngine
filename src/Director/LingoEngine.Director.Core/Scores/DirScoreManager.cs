using System;
using System.Collections.Generic;
using System.Linq;
using LingoEngine.Events;
using LingoEngine.Sprites;
using LingoEngine.Inputs;
using LingoEngine.Director.Core.Sprites;

namespace LingoEngine.Director.Core.Scores
{
    public class DirScoreSpriteChannel
    {
        public List<DirScoreSprite> Sprites { get; } = new();

        public DirScoreSprite? GetSpriteAtFrame(int frame)
        {
            return Sprites.FirstOrDefault(s => s.Sprite.BeginFrame <= frame && frame <= s.Sprite.EndFrame);
        }
    }

    public class DirScoreManager
    {
        private IDirSpritesManager? _spritesManager;
        private readonly Dictionary<int, DirScoreSpriteChannel> _channels = new();
        private readonly List<DirScoreSprite> _selected = new();

        private bool _dragging;
        private bool _dragBegin;
        private bool _dragEnd;
        private bool _dragMiddle;
        private int _mouseDownFrame = -1;

        public DirScoreManager()
        {
        }

        internal void SetSpritesManager(IDirSpritesManager manager)
        {
            _spritesManager = manager;
        }

        public void RegisterSprite(DirScoreSprite sprite)
        {
            int ch = sprite.Sprite.SpriteNum - 1;
            if (!_channels.TryGetValue(ch, out var channel))
            {
                channel = new DirScoreSpriteChannel();
                _channels[ch] = channel;
            }
            if (!channel.Sprites.Contains(sprite))
                channel.Sprites.Add(sprite);
        }

        private void ClearSelection()
        {
            var clone = _selected.ToList();
            foreach (var s in clone)
                s.IsSelected = false;
            _selected.Clear();
        }

        private void AddSelection(DirScoreSprite sprite)
        {
            if (_selected.Contains(sprite))
                return;
            if (_spritesManager != null && !_spritesManager.Key.ControlDown && !_spritesManager.Key.ShiftDown)
                ClearSelection();
            _selected.Add(sprite);
            sprite.IsSelected = true;
        }

        private DirScoreSprite? FindSprite(LingoSprite sprite)
        {
            int ch = sprite.SpriteNum - 1;
            if (_channels.TryGetValue(ch, out var channel))
                return channel.Sprites.FirstOrDefault(s => s.Sprite == sprite);
            return null;
        }

        public void SelectSprite(LingoSprite sprite)
        {
            var scoreSprite = FindSprite(sprite);
            if (scoreSprite != null)
                AddSelection(scoreSprite);
        }

        public void DeselectSprite(LingoSprite sprite)
        {
            var scoreSprite = _selected.FirstOrDefault(s => s.Sprite == sprite);
            if (scoreSprite == null)
                return;
            scoreSprite.IsSelected = false;
            _selected.Remove(scoreSprite);
        }

        public void HandleMouse(LingoMouseEvent mouseEvent, int channelNumber, int frameNumber)
        {
            if (!_channels.TryGetValue(channelNumber, out var channel))
                return;
            var sprite = channel.GetSpriteAtFrame(frameNumber);

            if (mouseEvent.Type == LingoMouseEventType.MouseDown && mouseEvent.Mouse.LeftMouseDown)
            {
                if (sprite != null)
                {
                    AddSelection(sprite);
                    if (frameNumber == sprite.Sprite.BeginFrame)
                    {
                        foreach (var s in _selected)
                            s.PrepareDragging(frameNumber);
                        _dragBegin = true;
                    }
                    else if (frameNumber == sprite.Sprite.EndFrame)
                    {
                        foreach (var s in _selected)
                            s.PrepareDragging(frameNumber);
                        _dragEnd = true;
                    }
                    else
                    {
                        foreach (var s in _selected)
                            s.PrepareDragging(frameNumber);
                        _dragMiddle = true;
                    }
                    _mouseDownFrame = frameNumber;
                }
                else
                {
                    ClearSelection();
                }
            }
            else if (mouseEvent.Type == LingoMouseEventType.MouseMove)
            {
                if (!_dragging)
                {
                    if (_mouseDownFrame >= 0 && Math.Abs(frameNumber - _mouseDownFrame) >= 1)
                        _dragging = true;
                    else
                        return;
                }
                if (_dragBegin)
                {
                    foreach (var s in _selected)
                        s.DragMoveBegin(frameNumber);
                }
                else if (_dragEnd)
                {
                    foreach (var s in _selected)
                        s.DragMoveEnd(frameNumber);
                }
                else if (_dragMiddle)
                {
                    foreach (var s in _selected)
                        s.DragMove(frameNumber);
                }
            }
            else if (mouseEvent.Type == LingoMouseEventType.MouseUp)
            {
                foreach (var s in _selected)
                    s.StopDragging();
                _dragging = _dragBegin = _dragEnd = _dragMiddle = false;
                _mouseDownFrame = -1;
            }
        }
    }
}
