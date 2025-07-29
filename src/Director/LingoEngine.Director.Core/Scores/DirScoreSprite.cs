using LingoEngine.Gfx;
using LingoEngine.Sprites;
using LingoEngine.Primitives;
using LingoEngine.Events;
using LingoEngine.Director.Core.Sprites;
using LingoEngine.Sounds;
using LingoEngine.Director.Core.Tools;
using System.Transactions;

namespace LingoEngine.Director.Core.Scores
{
    public enum DirScoreSpriteLabelType
    {
        Name,
        Member,
        Behavior,
        Location,
        Ink,
        Blend,
        Extended,
    }
    public class DirScoreSprite : IDisposable
    {
        private bool _isSelected;
        private bool _isDragging;
        private bool _isDraggingBegin;
        private bool _isDraggingEnd;
        private bool _isDraggingMiddle;
        private int _startDragFrameOffset = -1;
        private readonly IDirSpritesManager _spritesManager;

        public bool IsSingleFrameSprite { get; }
        public LingoSprite Sprite { get; }
        public LingoSprite2D? Sprite2D { get; }
        public bool RequireToRedraw { get; private set; } = true;
        public bool IsSelected
        {
            get => _isSelected; 
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                if (value)
                    _spritesManager.SelectSprite(Sprite);
                else
                    _spritesManager.DeselectSprite(Sprite);
                RequireRedraw();
            }
        }
        public LingoColor ColorCircleBorder { get; set; } = LingoColorList.Black;
        public LingoColor ColorCircle { get; set; } = LingoColorList.White;
        public LingoColor ColorBase { get; set; } = LingoColor.FromHex("#ccccff");
        public float X { get; private set; }
        public float OffsetY { get; private set; }
        public float Width { get; private set; }
        public float Height { get; private set; }
        public bool ShowLabel { get; set; } = true;
        public DirScoreSpriteLabelType LabelType { get; set; } = DirScoreSpriteLabelType.Member;

        public DirScoreSprite(LingoSprite sprite, IDirSpritesManager spritesManager)
        {
            Sprite = sprite;
            _spritesManager = spritesManager;
            Sprite2D = sprite as LingoSprite2D;
            Sprite.AnimationChanged += OnAnimationChanged;
            _spritesManager.SpritesSelection.SelectionCleared += OnSelectionCleared;
            spritesManager.Mediator.Subscribe<LingoSprite>(DirectorEventType.SpriteSelected, s =>
            {
                if (s == Sprite)
                    IsSelected = true;
                return true;
            });
            IsSingleFrameSprite = sprite.IsSingleFrame;
        }

        private void OnSelectionCleared()
        {
            if (IsSelected)
                IsSelected = false;
        }

        private void OnAnimationChanged() => RequireRedraw();
        private void RequireRedraw() => RequireToRedraw = true;

        public void Draw(LingoGfxCanvas canvas, float frameWidth, float channelHeight, float yOffset = 0)
        {
            if (!RequireToRedraw) return;
            RequireToRedraw = false;

            int ch = Sprite.SpriteNum - 1;
            if (ch < 0) return;
            OffsetY = yOffset;
            X = (Sprite.BeginFrame - 1) * frameWidth;
            Width = (Sprite.EndFrame - Sprite.BeginFrame + 1) * frameWidth;
            Height = channelHeight;
            int labelLeft = 8;
            var labelWidth = Width;

            // Draw Background
            canvas.DrawRect(new LingoRect(X, 0, X+ Width, channelHeight), GetBgColor());

            float radius = 3f;
            string name = Sprite.Name;
            if (Sprite.BeginFrame != Sprite.EndFrame)
            {
                // Draw keyframes
                var startCenter = new LingoPoint(X + 3f, OffsetY + Height / 2f);
                var endCenter = new LingoPoint(X + Width - 3f, OffsetY + Height / 2f);
                canvas.DrawCircle(startCenter, radius, ColorCircle);
                canvas.DrawCircle(endCenter, radius, ColorCircle);
                canvas.DrawArc(startCenter, radius, 0, 360, 8, ColorCircleBorder);
                canvas.DrawArc(endCenter, radius, 0, 360, 8, ColorCircleBorder);
                if (Sprite is LingoSprite2D)
                    name = GetName();
                labelWidth = Width - labelLeft - frameWidth;
            }
            else
            {
                labelLeft = 0;
            }
            // Draw name
            canvas.DrawText(new LingoPoint(X + labelLeft, 11), name, null, LingoColorList.Black, 9, (int)labelWidth);
        }

        public bool IsPointInsideSprite(float x, float y)
        {
            if (x < X || x > X + Width || y < OffsetY || y > OffsetY + Height)
                return false;
            return true;
        }
        
        public void HandleMouse(LingoMouseEvent mouseEvent, int frameNumber)
        {
            if (mouseEvent.Type == LingoMouseEventType.MouseDown)
            {
                if (mouseEvent.Mouse.LeftMouseDown)
                    HandleLeftMouseDown(mouseEvent,frameNumber);
            }
            else if (mouseEvent.Type == LingoMouseEventType.MouseUp)
            {
                StopDragging();
            }
            if (mouseEvent.Type == LingoMouseEventType.MouseMove)
                HandleMouseMove(frameNumber);
        }

       
      

        private void HandleLeftMouseDown(LingoMouseEvent mouseEvent, int frameNumber)
        {
            // Single frame sprites
            if (IsSingleFrameSprite)
            {
                if (IsSelected)
                {
                    _isDragging = true;
                    _isDraggingMiddle = true;
                    _startDragFrameOffset = frameNumber - Sprite.BeginFrame;
                }
                else
                {
                    IsSelected = true;
                    StopDragging();
                }
                return;
            }
            // multi frame sprites
            if (frameNumber == Sprite.BeginFrame)
            {
                _isDragging = true;
                _isDraggingBegin = true;
            }
            else if (frameNumber == Sprite.EndFrame)
            {
                _isDragging = true;
                _isDraggingEnd = true;
            } 
            else if (frameNumber > Sprite.BeginFrame && frameNumber < Sprite.EndFrame  && IsSelected)
            {
                _isDragging = true;
                _isDraggingMiddle = true;
                _startDragFrameOffset = frameNumber - Sprite.BeginFrame;
            }
            // Todo : keyframes
            else
            {
                if (!IsSelected)
                    IsSelected = true;
                else
                    IsSelected = false;
                StopDragging();
            }
        }
        private void HandleMouseMove(int frameNumber)
        {
            if (!_isDragging) return;
            if (_isDraggingBegin)
            {
                if (frameNumber <= Sprite.EndFrame && frameNumber > 0)
                    Sprite.BeginFrame = frameNumber;
                return;
            } 
            if (_isDraggingMiddle)
            {
                var duration = Sprite.EndFrame - Sprite.BeginFrame;
                Sprite.BeginFrame = frameNumber - _startDragFrameOffset;
                Sprite.EndFrame = Sprite.BeginFrame + duration;
                return;
            } 
            if (_isDraggingEnd)
            {
                if (frameNumber >= Sprite.BeginFrame && frameNumber > 0)
                    Sprite.EndFrame = frameNumber;
                return;
            }
        }
        public void StopDragging()
        {
            _isDragging = false;
            _isDraggingBegin = false;
            _isDraggingEnd = false;
            _isDraggingMiddle = false;
        }


        private string GetName()
        {
            if (!ShowLabel || Sprite2D == null)
                return Sprite.Name;

            // For 2D spritesd
            switch (LabelType)
            {
                case DirScoreSpriteLabelType.Name: return Sprite2D.Name;
                case DirScoreSpriteLabelType.Member: return Sprite2D.Member?.Name ?? string.Empty;
                case DirScoreSpriteLabelType.Behavior: return Sprite2D.Behaviors.Count > 0 ? string.Join(",",Sprite2D.Behaviors.Select(x => x.Name)) : string.Empty;
                case DirScoreSpriteLabelType.Location: return $"{Sprite2D.LocH},{Sprite2D.LocV} ({Sprite2D.LocZ})";
                case DirScoreSpriteLabelType.Ink: return Sprite2D.InkType.ToString();
                case DirScoreSpriteLabelType.Blend: return Sprite2D.Blend.ToString();
                case DirScoreSpriteLabelType.Extended: return Sprite2D.Name;
                default:
                    return "";
            }
        }

        private LingoColor GetBgColor()
        {
            var bgColor = ColorBase;
            if (Sprite.Lock)
                bgColor = ColorBase.Lighten(0.7f);
            if (IsSelected)
                bgColor = ColorBase.Darken(0.25f);
            return bgColor;
        }

        public void Dispose()
        {
            Sprite.AnimationChanged -= OnAnimationChanged;
            _spritesManager.SpritesSelection.SelectionCleared -= OnSelectionCleared;
        }
    }
}
