using LingoEngine.Director.Core.Sprites;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Director.Core.Windowing;
using LingoEngine.Members;
using LingoEngine.Movies;
using LingoEngine.Sprites;
using LingoEngine.Director.Core.Styles;
using System.Collections.Generic;
using AbstUI.Primitives;
using AbstUI.Components;
using AbstUI.Windowing;

namespace LingoEngine.Director.Core.Scores
{
    public interface IDirScoreChannelFramework
    {
        void RequireRedraw();
    }

    public interface IDirScoreChannel
    {
        bool Visible { get; }
        APoint Position { get; }
        APoint Size { get; }
    }

    public abstract class DirScoreChannel : IDisposable, IDirScoreChannel
    {
        protected readonly DirScoreGfxValues _gfxValues;
        protected readonly IDirSpritesManager _spritesManager;
        protected readonly IDirScoreManager _scoreManager;
        protected readonly AbstGfxCanvas _canvas;
        protected LingoMovie? _movie;
        protected float _scrollX;
        protected int _lastFrame = -1;
        protected bool _dirty = true;
        protected bool _spriteDirty = true;
        protected bool _hasDirtySpriteList = true;
        protected IDirScoreChannelFramework? _framework;
        protected Func<string, IAbstFrameworkPanel, IAbstWindowDialogReference?> _showConfirmDialog;

        public bool HasDirtySpriteList { get => _hasDirtySpriteList; set => _hasDirtySpriteList = value; }
        public bool Visible { get; internal set; } = true;
        public APoint Position { get; internal set; }
        public APoint Size { get; set; }
        public int SpriteNumWithChannelNum { get; }
        public bool IsSingleFrame { get; protected set; }
        public virtual T Framework<T>() where T : IAbstFrameworkNode => _canvas.Framework<T>();
        public virtual IAbstFrameworkNode FrameworkObj => _canvas.FrameworkObj;

        public bool ShowPreview { get; protected set; }
        public int PreviewBegin { get; protected set; }
        public int PreviewEnd { get; protected set; }

        public bool ShowSelectionRect { get; private set; }
        public int SelectionBegin { get; private set; }
        public int SelectionEnd { get; private set; }


#pragma warning disable CS8618 
        protected DirScoreChannel(int spriteNumWithChannelNum, IDirScoreManager scoreManager)
#pragma warning restore CS8618 
        {
            _spritesManager = scoreManager.SpritesManager;
            _scoreManager = scoreManager;
            _gfxValues = _scoreManager.GfxValues;
            SpriteNumWithChannelNum = spriteNumWithChannelNum;
            _canvas = _spritesManager.Factory.CreateGfxCanvas("channel_" + spriteNumWithChannelNum, 800, _gfxValues.ChannelHeight);
            ((DirScoreManager)_scoreManager).RegisterChannel(this);
        }

        public void Init(IDirScoreChannelFramework framework)
        {
            _framework = framework;
        }
        public virtual void Dispose()
        {
            ((DirScoreManager)_scoreManager).UnregisterChannel(this);
            _canvas.Dispose();
            _framework = null;
        }

        public void RequireRedraw() => MarkDirty();
        protected void MarkDirty()
        {
            _dirty = true;
            _framework?.RequireRedraw();
        }

        public virtual float ScrollX
        {
            get => _scrollX;
            set
            {
                _scrollX = value;
            }
        }

        public virtual void SetMovie(LingoMovie? movie)
        {
            if (_movie != null)
                UnsubscribeMovie(_movie);
            _movie = movie;
            if (_movie != null)
                SubscribeMovie(_movie);
            UpdateSize();
        }
        public abstract DirScoreSprite? FindSprite(LingoSprite sprite);
        public abstract DirScoreSprite? GetSpriteAtFrame(int frame);
        public abstract IEnumerable<DirScoreSprite> GetSprites();
        protected virtual void SubscribeMovie(LingoMovie movie) { }
        protected virtual void UnsubscribeMovie(LingoMovie movie) { }

        public void UpdateSize()
        {
            if (_movie == null) return;
            float width = _gfxValues.LeftMargin + _movie.FrameCount * _gfxValues.FrameWidth;
            Size = new APoint(width, _gfxValues.ChannelHeight);
            _canvas.Width = Size.X;
            _canvas.Height = Size.Y;
        }
        internal virtual void ShowCreateSpriteDialog(int frameNumber, Action<LingoSprite?> newSprite)
        {
        }
        internal virtual void ShowSpriteDialog(LingoSprite sprite)
        {
        }


        public virtual void Draw()
        {
        }

        internal void SetShowDialogMethod(Func<string, IAbstFrameworkPanel, IAbstWindowDialogReference?> showConfirmDialog)
        {
            _showConfirmDialog = showConfirmDialog;
        }
        /// <summary>
        /// Retrurns false if beginsprite is inside existing sprite.
        /// </summary>
        internal virtual bool DrawPreview(int frameNumber)
        {
            return true;
        }

        internal virtual void StopPreview()
        {

        }

        internal virtual bool CanAcceptSpriteRange(DirScoreSprite? sprite, int begin, int end) => true;

        internal virtual bool DrawMovePreview(int begin, int end, DirScoreSprite? ignore = null) => true;

        internal void SetSelectionRect(int begin, int end)
        {
            SelectionBegin = begin;
            SelectionEnd = end;
            ShowSelectionRect = true;
            RequireRedraw();
        }

        internal void ClearSelectionRect()
        {
            if (!ShowSelectionRect) return;
            ShowSelectionRect = false;
            RequireRedraw();
        }

        internal abstract void ShowSpriteInfo(DirScoreSpriteLabelType type);
    }




    internal abstract partial class DirScoreChannel<TSpriteManager, TSpriteUI, TSprite> : DirScoreChannel, IDirScoreChannel
        where TSpriteUI : DirScoreSprite<TSprite>
        where TSpriteManager : ILingoSpriteManager<TSprite>
        where TSprite : LingoSprite
    {
        protected readonly List<TSpriteUI> _spriteUIs = new();
        protected readonly IDirectorEventMediator _mediator;
        private readonly bool _subscribeToSpritelistChange;
        protected TSpriteManager? _manager;
        protected TSpriteUI? _selectedUI;


        //protected readonly HashSet<Vector2I> _selectedCells = new();
        //protected Vector2I? _lastSelectedCell = null;
        protected int _dragFrame;

        public List<TSpriteUI> SpriteUIs => _spriteUIs;


        protected DirScoreChannel(int spriteNumWithChannel, IDirScoreManager scoreManager, bool subscribeToSpritelistChange = true)
            : base(spriteNumWithChannel, scoreManager)
        {

            _mediator = scoreManager.SpritesManager.Mediator;
            _subscribeToSpritelistChange = subscribeToSpritelistChange;

            //AddChild(_canvas);
            //MouseFilter = MouseFilterEnum.Stop;
        }
        public override void Dispose()
        {
            if (_manager != null && _subscribeToSpritelistChange) _manager.SpriteListChanged -= SpriteListChanged;
            base.Dispose();
        }


        protected override void SubscribeMovie(LingoMovie movie)
        {
            _manager = GetManager(movie);
            if (_manager == null) return;
            // For sprite2D we register to the parent container to boost performance.
            if (_subscribeToSpritelistChange) _manager.SpriteListChanged += SpriteListChanged;
            _hasDirtySpriteList = true;
        }

        protected override void UnsubscribeMovie(LingoMovie movie)
        {
            if (_manager == null) return;
            if (_subscribeToSpritelistChange) _manager.SpriteListChanged -= SpriteListChanged;
        }
        internal void SpriteListChanged(int spriteNumWithChannelNum)
        {
            if (spriteNumWithChannelNum != SpriteNumWithChannelNum) return;
            _hasDirtySpriteList = true;
            MarkDirty();
        }


        protected abstract TSpriteUI CreateUISprite(TSprite sprite, IDirSpritesManager spritesManager);

        protected virtual void OnSpriteClicked(TSpriteUI sprite)
        {
            _mediator.RaiseSpriteSelected(sprite.Sprite);
        }

        private void RedrawAllSprites()
        {
            foreach (var spriteUI in _spriteUIs)
                spriteUI.Dispose();

            _spriteUIs.Clear();
            if (_manager == null) return;
            var sprites = _manager.GetAllSpritesBySpriteNumAndChannel(SpriteNumWithChannelNum);
            foreach (var s in sprites)
            {
                var uiSprite = CreateUISprite(s, _spritesManager);
                uiSprite.Channel = this;
                _spriteUIs.Add(uiSprite);
            }
            MarkDirty();
            _hasDirtySpriteList = false;
        }
        public override DirScoreSprite? GetSpriteAtFrame(int frame)
        {
            return _spriteUIs.FirstOrDefault(s => s.Sprite.BeginFrame <= frame && frame <= s.Sprite.EndFrame);
        }
        public override DirScoreSprite? FindSprite(LingoSprite sprite)
            => _spriteUIs.FirstOrDefault(s => s.Sprite == sprite);
        protected abstract TSpriteManager GetManager(LingoMovie movie);

        public override IEnumerable<DirScoreSprite> GetSprites() => _spriteUIs;

        /// </ummary>
        /// Retrurns false if beginsprite is inside existing sprite.
        /// </summary>
        internal override bool DrawPreview(int frameNumber)
        {
            if (_movie == null || _manager == null) return true;
            var isSingleFrameSprite = IsSingleFrame;
            PreviewBegin = frameNumber;
            var endFrame = _movie.FrameLabels.GetNextLabelFrame(frameNumber);
            if (endFrame > _movie.FrameCount)
                endFrame = _movie.FrameCount;
            PreviewEnd = endFrame;
            if (PreviewEnd < PreviewBegin)
                PreviewEnd = PreviewBegin + 30;
            var beginInSprite = _spriteUIs.FirstOrDefault(x => x.IsFrameInSprite(PreviewBegin));
            if (beginInSprite != null)
            {
                ShowPreview = false;
                RequireRedraw();
                if (isSingleFrameSprite) return true; // if its a single frame, we may not move it to a second channel
                return false;
            }
            if (isSingleFrameSprite)
                PreviewEnd = PreviewBegin;
            else
            {
                var endInSprite = _spriteUIs
                .Where(x => x.IsFrameRangeInSprite(PreviewBegin, PreviewEnd))
                .OrderBy(x => x.Sprite.BeginFrame)
                .FirstOrDefault();
                if (endInSprite != null)
                    PreviewEnd = endInSprite.Sprite.BeginFrame - 1;
            }

            ShowPreview = true;
            RequireRedraw();
            return true;
        }
        internal override void StopPreview()
        {
            ShowPreview = false;
            RequireRedraw();
        }

        /// <summary>
        /// Checks if a sprite range can be placed on this channel without colliding with existing sprites.
        /// </summary>
        /// <param name="sprite">Sprite to ignore from collision checks, if any.</param>
        /// <param name="begin">Proposed first frame.</param>
        /// <param name="end">Proposed last frame.</param>
        /// <returns><c>true</c> if the range does not overlap any other sprite; otherwise, <c>false</c>.</returns>
        internal override bool CanAcceptSpriteRange(DirScoreSprite? sprite, int begin, int end)
        {
            foreach (var sp in _spriteUIs)
            {
                if (ReferenceEquals(sp, sprite))
                    continue;
                if (!(end < sp.Sprite.BeginFrame || begin > sp.Sprite.EndFrame))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Draws a preview rectangle for a potential sprite move if the target range is free.
        /// </summary>
        /// <param name="begin">Start frame of the preview range.</param>
        /// <param name="end">End frame of the preview range.</param>
        /// <param name="ignore">Optional sprite to exclude during overlap checks.</param>
        /// <returns><c>true</c> if the preview was shown; otherwise, <c>false</c>.</returns>
        internal override bool DrawMovePreview(int begin, int end, DirScoreSprite? ignore = null)
        {
            if (!CanAcceptSpriteRange(ignore, begin, end))
            {
                StopPreview();
                return false;
            }

            PreviewBegin = begin;
            PreviewEnd = end;
            ShowPreview = true;
            RequireRedraw();
            return true;
        }
        public override void Draw()
        {
            if (_hasDirtySpriteList)
                RedrawAllSprites();

            _canvas.Clear(AColors.Transparent);

            if (_movie == null) return;

            int channelCount = _movie.MaxSpriteChannelCount;

            foreach (var sp in _spriteUIs)
                sp.Draw(_canvas, _gfxValues.FrameWidth, _gfxValues.ChannelHeight);


            if (ShowPreview)
            {
                float px = (PreviewBegin - 1) * _gfxValues.FrameWidth;
                float pw = PreviewEnd * _gfxValues.FrameWidth;
                _canvas.DrawRect(new ARect(px, 0, pw, _gfxValues.ChannelHeight), new AColor(0, 120, 120, 80), false, 1);
            }

            if (ShowSelectionRect)
            {
                float sx = (SelectionBegin - 1) * _gfxValues.FrameWidth;
                float ex = SelectionEnd * _gfxValues.FrameWidth;
                _canvas.DrawRect(new ARect(sx, 0, ex, _gfxValues.ChannelHeight), DirectorColors.BlueSelectColorSemiTransparent, false, 1);
            }
            _dirty = false;
        }

        internal override void ShowSpriteInfo(DirScoreSpriteLabelType type)
        {
            foreach (var spriteUI in _spriteUIs)
            {
                spriteUI.LabelType = type;
            }
        }
    }
}
