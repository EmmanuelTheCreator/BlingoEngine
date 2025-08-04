using LingoEngine.Events;
using LingoEngine.Sprites;
using LingoEngine.Director.Core.Sprites;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Director.Core.Tools;

namespace LingoEngine.Director.Core.Scores
{
    

    public interface IDirScoreManager
    {
        DirScoreGfxValues GfxValues { get; }
        IDirSpritesManager SpritesManager { get; }
        ILingoFrameworkFactory Factory { get; }
        void DeselectSprite(LingoSprite sprite);
        void HandleMouse(LingoMouseEvent mouseEvent, int channelNumber, int frameNumber);
        void SelectSprite(LingoSprite sprite);
       
    }

    public class DirScoreManager : IDirScoreManager
    {
        private IDirSpritesManager? _spritesManager;
        private readonly Dictionary<int, DirScoreChannel> _channels = new();
        private readonly List<DirScoreSprite> _selected = new();
        private readonly IDirectorEventMediator _directorEventMediator;
        private DirScoreSprite? _lastAddedSprite;
        private bool _dragging;
        private bool _dragBegin;
        private bool _dragEnd;
        private bool _dragMiddle;
        private int _mouseDownFrame = -1;
        public DirScoreGfxValues GfxValues { get; } = new();
        public IDirSpritesManager SpritesManager => _spritesManager!;
        public ILingoFrameworkFactory Factory { get; }
        public DirScoreManager(ILingoFrameworkFactory factory, IDirectorEventMediator directorEventMediator)
        {
            Factory = factory;
            _directorEventMediator = directorEventMediator;
        }

        internal void SetSpritesManager(IDirSpritesManager manager)
        {
            _spritesManager = manager;
        }
        public void RegisterChannel(DirScoreChannel channel)
        {
            if (_channels.ContainsKey(channel.SpriteNumWithChannelNum))
                throw new InvalidOperationException($"Channel with sprite number {channel.SpriteNumWithChannelNum} already exists.");
            _channels[channel.SpriteNumWithChannelNum] = channel;
        } 
        public void UnregisterChannel(DirScoreChannel channel)
        {
            if (!_channels.ContainsKey(channel.SpriteNumWithChannelNum))
                return;
            _channels.Remove(channel.SpriteNumWithChannelNum);
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
            if (sprite.IsLocked) return;
            if (_selected.Contains(sprite))
            {
                if (_spritesManager != null && (_spritesManager.Key.ControlDown || _spritesManager.Key.ShiftDown))
                {
                    sprite.IsSelected = false;
                    _selected.Remove(sprite);
                }
                return;
            }
            sprite.IsSelected = true;
            _selected.Add(sprite);  
            _lastAddedSprite = sprite;
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
            if (sprite.Lock) return;
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
            var spriteScore = channel.GetSpriteAtFrame(frameNumber);

            if (mouseEvent.Type == LingoMouseEventType.MouseDown && mouseEvent.Mouse.LeftMouseDown)
            {
                if (spriteScore != null)
                {
                    if (spriteScore.IsLocked)
                    {
                        mouseEvent.Mouse.SetCursor(Inputs.LingoMouseCursor.NotAllowed);
                        _directorEventMediator.RaiseSpriteSelected(spriteScore.Sprite);
                        _mouseDownFrame = -1;
                        return;
                    }
                    AddSelection(spriteScore);
                    if (frameNumber == spriteScore.Sprite.BeginFrame)
                    {
                        foreach (var s in _selected)
                            s.PrepareDragging(frameNumber);
                        _dragBegin = true;
                    }
                    else if (frameNumber == spriteScore.Sprite.EndFrame)
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
                    {
                        _dragging = true;
                        mouseEvent.Mouse.SetCursor(Inputs.LingoMouseCursor.Drag);
                    }
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
                if (_spritesManager != null && !_dragging && !_spritesManager.Key.ControlDown && !_spritesManager.Key.ShiftDown && _lastAddedSprite == null)
                    ClearSelection();
                else
                {
                    foreach (var s in _selected)
                    {
                        s.StopDragging();
                        if (s.Channel != null)
                            s.Channel.RequireRedraw();
                    }
                }
                _dragging = _dragBegin = _dragEnd = _dragMiddle = false;
                _mouseDownFrame = -1;
                _lastAddedSprite = null;
                mouseEvent.Mouse.SetCursor(Inputs.LingoMouseCursor.Arrow);
            }
        }

       
    }
}
