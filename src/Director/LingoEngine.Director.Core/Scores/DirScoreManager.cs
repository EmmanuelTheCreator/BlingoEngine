using LingoEngine.Events;
using LingoEngine.Sprites;
using LingoEngine.Director.Core.Sprites;

namespace LingoEngine.Director.Core.Scores
{
    

    public interface IDirScoreManager
    {
        IDirSpritesManager SpritesManager { get; }

        void DeselectSprite(LingoSprite sprite);
        void HandleMouse(LingoMouseEvent mouseEvent, int channelNumber, int frameNumber);
        void SelectSprite(LingoSprite sprite);
       
    }

    public class DirScoreManager : IDirScoreManager
    {
        private IDirSpritesManager? _spritesManager;
        private readonly Dictionary<int, DirScoreChannel> _channels = new();
        private readonly List<DirScoreSprite> _selected = new();

        private bool _dragging;
        private bool _dragBegin;
        private bool _dragEnd;
        private bool _dragMiddle;
        private int _mouseDownFrame = -1;

        public IDirSpritesManager SpritesManager => _spritesManager!;

        public DirScoreManager()
        {
        }

        internal void SetSpritesManager(IDirSpritesManager manager)
        {
            _spritesManager = manager;
        }
        public void RegisterChannel(DirScoreChannel channel)
        {
            if (_channels.ContainsKey(channel.SpriteNum))
                throw new InvalidOperationException($"Channel with sprite number {channel.SpriteNum} already exists.");
            _channels[channel.SpriteNum] = channel;
        } 
        public void UnregisterChannel(DirScoreChannel channel)
        {
            if (!_channels.ContainsKey(channel.SpriteNum))
                return;
            _channels.Remove(channel.SpriteNum);
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
                return channel.FindSprite(sprite);
               
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
                    if (_selected.Count > 0)
                        channel.RequireRedraw();
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
