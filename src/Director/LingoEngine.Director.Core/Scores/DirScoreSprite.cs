using LingoEngine.Gfx;
using LingoEngine.Sprites;
using LingoEngine.Primitives;
using LingoEngine.Director.Core.Sprites;
using LingoEngine.Director.Core.Tools;
using System.Linq;

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
    public class DirScoreSprite<TSprite> : DirScoreSprite
        where TSprite : LingoSprite
    {
        public TSprite SpriteT { get; private set; }


#pragma warning disable CS8618
        public DirScoreSprite(TSprite sprite, IDirSpritesManager spritesManager) : base(sprite, spritesManager)
#pragma warning restore CS8618 
        {
            SpriteT = sprite;
        }

    }
    public class DirScoreSprite : IDisposable
    {
        private bool _isSelected;
        private int _dragStartFrameOffset = -1;
        private int _dragStartBeginFrame;
        private int _dragStartEndFrame;
        private int _dragStartChannel;
        private DirScoreSpriteLabelType labelType = DirScoreSpriteLabelType.Member;
        private readonly IDirSpritesManager _spritesManager;
        internal DirScoreChannel? Channel { get; set; }
        public bool IsSingleFrameSprite { get; }
        public LingoSprite Sprite { get; }
        public LingoSprite2D? Sprite2D { get; }
        public bool RequireToRedraw { get; private set; } = true;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetSelected(value);
        }
        public LingoColor ColorCircleBorder { get; set; } = LingoColorList.Black;
        public LingoColor ColorCircle { get; set; } = LingoColorList.White;
        public LingoColor ColorBase { get; set; } = LingoColor.FromHex("#ccccff");
        public float X { get; private set; }
        public float OffsetY { get; private set; }
        public float Width { get; private set; }
        public float Height { get; private set; }
        public bool ShowLabel { get; set; } = true;
        public DirScoreSpriteLabelType LabelType { get => labelType; set { labelType = value; RequireRedraw(); } }
        public bool IsLocked => Sprite.Lock;

        internal int DragStartChannel => _dragStartChannel;
        internal int DragStartBeginFrame => _dragStartBeginFrame;
        internal int DragStartEndFrame => _dragStartEndFrame;

        private IEnumerable<int>? _KeyFrames;
        private bool _dragKeyFrame;
        private int _dragKeyFrameStart;
        private int _dragKeyFramePreview;

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
                SetSelected(false, false);
        }

        private void OnAnimationChanged()
        {
            _KeyFrames = null;
            RequireRedraw();
        }
        private void RequireRedraw()
        {
            RequireToRedraw = true;
            //if (Sprite.IsDeleted)
            //    Channel?.RequireFullRedraw();
            //else
            Channel?.RequireRedraw();
        }

        internal void SetSelected(bool value, bool notify = true)
        {
            if (_isSelected == value) return;
            _isSelected = value;
            if (notify)
            {
                if (value)
                    _spritesManager.SelectSprite(Sprite);
                else
                    _spritesManager.DeselectSprite(Sprite);
            }
            RequireRedraw();
        }

        public void Draw(LingoGfxCanvas canvas, float frameWidth, float channelHeight, float yOffset = 0)
        {
            //if (!RequireToRedraw) return;
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
            canvas.DrawRect(new LingoRect(X, 0, X + Width, channelHeight), GetBgColor());

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
            var keyFrames = Sprite2D?.GetKeyframes();
            if (keyFrames != null)
            {
                _KeyFrames = keyFrames.Select(x => x.Frame + Sprite.BeginFrame).ToArray();
                foreach (var keyFrame in _KeyFrames)
                {
                    if (keyFrame == Sprite.BeginFrame || keyFrame == Sprite.EndFrame)
                        continue; // already drawn
                    var centerX = X + 3f + (keyFrame - Sprite.BeginFrame) * frameWidth;
                    var keyframeCenter = new LingoPoint(centerX, OffsetY + Height / 2f);
                    var color = ColorCircleBorder;
                    if (_dragKeyFrame && keyFrame == _dragKeyFrameStart && _dragKeyFrameStart != _dragKeyFramePreview)
                        color = ColorCircleBorder.Lighten(0.9f);
                    canvas.DrawCircle(keyframeCenter, radius, ColorCircle);
                    canvas.DrawArc(keyframeCenter, radius, 0, 360, 8, color);
                }
                if (_dragKeyFrame && _dragKeyFrameStart != _dragKeyFramePreview)
                {
                    var previewCenter = new LingoPoint(X + 3f + (_dragKeyFramePreview - Sprite.BeginFrame) * frameWidth, OffsetY + Height / 2f);
                    canvas.DrawCircle(previewCenter, radius, ColorCircle);
                    canvas.DrawArc(previewCenter, radius, 0, 360, 8, ColorCircleBorder);
                }
            }
            else
            {
                _KeyFrames = null;
            }
        }

        public bool IsFrameInSprite(int frame)
            => Sprite.BeginFrame <= frame && Sprite.EndFrame >= frame;
        public bool IsFrameRangeInSprite(int beginFrame, int endFrame)
            =>
            (Sprite.BeginFrame >= beginFrame && beginFrame <= Sprite.EndFrame)
            || (Sprite.BeginFrame >= endFrame && endFrame <= Sprite.EndFrame)
            ;

        public bool IsPointInsideSprite(float x, float y)
        {
            if (x < X || x > X + Width || y < OffsetY || y > OffsetY + Height)
                return false;
            return true;
        }

        public void PrepareDragging(int startDragFrameOffset)
        {
            _dragStartFrameOffset = startDragFrameOffset;
            _dragStartBeginFrame = Sprite.BeginFrame;
            _dragStartEndFrame = Sprite.EndFrame;
            _dragStartChannel = Sprite.SpriteNumWithChannel;
        }

        public void DragMoveBegin(int frameOffset)
        {
            if (Sprite.Lock) return;
            if (Sprite.IsSingleFrame)
            {
                DragMove(frameOffset);
                return;
            }
            int newBegin = _dragStartBeginFrame + (frameOffset - _dragStartFrameOffset);
            if (newBegin <= Sprite.EndFrame && newBegin > 0)
                Sprite.BeginFrame = newBegin;
        }

        public void DragMove(int frameOffset)
        {
            if (Sprite.Lock) return;
            int delta = frameOffset - _dragStartFrameOffset;
            Sprite.BeginFrame = _dragStartBeginFrame + delta;
            Sprite.EndFrame = _dragStartEndFrame + delta;
        }

        public void DragMoveEnd(int frameOffset)
        {
            if (Sprite.Lock) return;
            if (Sprite.IsSingleFrame)
            {
                DragMove(frameOffset);
                return;
            }
            int newEnd = _dragStartEndFrame + (frameOffset - _dragStartFrameOffset);
            if (newEnd >= Sprite.BeginFrame && newEnd > 0)
                Sprite.EndFrame = newEnd;
        }

        public void StopDragging(bool checkCollision = true)
        {
            if (checkCollision && Channel != null)
            {
                if (!Channel.CanAcceptSpriteRange(this, Sprite.BeginFrame, Sprite.EndFrame))
                {
                    Sprite.BeginFrame = _dragStartBeginFrame;
                    Sprite.EndFrame = _dragStartEndFrame;
                }
            }

            _dragStartFrameOffset = -1;
            _dragStartBeginFrame = Sprite.BeginFrame;
            _dragStartEndFrame = Sprite.EndFrame;
            _dragStartChannel = Sprite.SpriteNumWithChannel;
        }

        internal bool IsKeyFrame(int frame)
        {
            if (_KeyFrames == null && Sprite2D != null)
                _KeyFrames = Sprite2D.GetKeyframes()?.Select(k => k.Frame + Sprite.BeginFrame).ToArray();
            return _KeyFrames != null && _KeyFrames.Contains(frame);
        }

        internal void PrepareKeyFrameDragging(int frame)
        {
            _dragKeyFrame = true;
            _dragKeyFrameStart = frame;
            _dragKeyFramePreview = frame;
        }

        internal void DragKeyFrame(int frame)
        {
            if (!_dragKeyFrame || _KeyFrames == null)
                return;
            if (frame <= Sprite.BeginFrame || frame >= Sprite.EndFrame)
                return;
            if (_dragKeyFramePreview == frame)
                return;
            _dragKeyFramePreview = frame;
            RequireRedraw();
        }

        internal void StopKeyFrameDragging()
        {
            if (!_dragKeyFrame)
                return;
            if (_dragKeyFramePreview != _dragKeyFrameStart && Sprite2D != null)
                Sprite2D.MoveKeyFrame(_dragKeyFrameStart - Sprite.BeginFrame, _dragKeyFramePreview - Sprite.BeginFrame);
            _KeyFrames = null;
            _dragKeyFrame = false;
            RequireRedraw();
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
                case DirScoreSpriteLabelType.Behavior: return Sprite2D.Behaviors.Count > 0 ? string.Join(",", Sprite2D.Behaviors.Select(x => x.Name)) : string.Empty;
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
