using LingoEngine.Director.Core.Sprites;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Events;
using LingoEngine.Gfx;
using LingoEngine.Movies;
using LingoEngine.Primitives;
using LingoEngine.Sprites;

namespace LingoEngine.Director.Core.Scores
{
    public interface IDirScoreChannelFramework
    {
        void RequireRedraw();
    }

    public interface IDirScoreChannel
    {
        bool Visible { get; }
        LingoPoint Position { get; }
        LingoPoint Size { get; }
    }
  
    public abstract class DirScoreChannel : IDisposable, IDirScoreChannel
    {
        protected LingoMovie? _movie;
        protected readonly DirScoreGfxValues _gfxValues;
        protected float _scrollX;
        protected bool _dirty = true;
        protected bool _spriteDirty = true;
        protected bool _spriteListDirty;
        protected int _lastFrame = -1;
        protected readonly IDirSpritesManager _spritesManager;
        protected readonly IDirScoreManager _scoreManager;
        protected readonly LingoGfxCanvas _canvas;
        protected bool _hasDirtySprites = true;
        private IDirScoreChannelFramework _framework;

        public bool HasDirtySprites { get => _hasDirtySprites; set => _hasDirtySprites = value; }
        internal bool SpriteListDirty { get => _spriteListDirty; set => _spriteListDirty = value; }
        

        public bool Visible { get; internal set; }
        public LingoPoint Position { get; internal set; }
        public LingoPoint Size { get; set; }

        public int SpriteNum { get; }

        public virtual T Framework<T>() where T : ILingoFrameworkGfxNode => _canvas.Framework<T>();
        public virtual ILingoFrameworkGfxNode FrameworkObj => _canvas.FrameworkObj;

        

#pragma warning disable CS8618 
        protected DirScoreChannel(int spriteNum, IDirScoreManager scoreManager)
#pragma warning restore CS8618 
        {
            _spritesManager = scoreManager.SpritesManager;
            _scoreManager = scoreManager;
            _gfxValues = _spritesManager.GfxValues;
            SpriteNum = spriteNum;
            _canvas = _spritesManager.Factory.CreateGfxCanvas("channel_" + spriteNum, 800, _spritesManager.GfxValues.ChannelHeight);
            ((DirScoreManager)_scoreManager).RegisterChannel(this);
        }

        public void Init(IDirScoreChannelFramework framework)
        {
            _framework = framework;
        }


        public void RequireRedraw() => MarkDirty();
        protected void MarkDirty()
        {
            _dirty = true;
            _framework.RequireRedraw();
            //QueueRedraw();
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
            {
                SubscribeMovie(_movie);

            }
        }
        public abstract DirScoreSprite? FindSprite(LingoSprite sprite);
        public abstract DirScoreSprite? GetSpriteAtFrame(int frame);
        protected virtual void SubscribeMovie(LingoMovie movie) { }
        protected virtual void UnsubscribeMovie(LingoMovie movie) { }

        public virtual bool HandleMouseEvent(LingoMouseEvent mouseEvent, int currentFrame)
        {
            return false;
        }

        public void UpdateSize()
        {
            if (_movie == null) return;
            float width = _gfxValues.LeftMargin + _movie.FrameCount * _gfxValues.FrameWidth;
            Size = new LingoPoint(width, _gfxValues.ChannelHeight);
            //CustomMinimumSize = Size;
            _canvas.Width = Size.X;
            _canvas.Height = Size.Y;
            //_canvas.QueueRedraw();
        }
       
        public void Dispose()
        {
            ((DirScoreManager)_scoreManager).UnregisterChannel(this);
            _canvas.Dispose();
        }

        public virtual void Draw()
        {
        }

      
    }




    internal abstract partial class DirScoreChannel<TSpriteManager, TSpriteUI, TSprite> : DirScoreChannel, IDirScoreChannel
        where TSpriteUI : DirScoreSprite<TSprite>
        where TSpriteManager : ILingoSpriteManager<TSprite>
        where TSprite : LingoSprite
    {
        protected readonly List<TSpriteUI> _sprites = new();
        protected readonly IDirectorEventMediator _mediator;
        protected TSpriteManager? _manager;
        protected TSpriteUI? _selected;
       

        //protected readonly HashSet<Vector2I> _selectedCells = new();
        //protected Vector2I? _lastSelectedCell = null;
        protected int _dragFrame;
        public List<TSpriteUI> Sprites => _sprites;
        protected DirScoreChannel(int spriteNum, IDirScoreManager scoreManager)
            : base(spriteNum, scoreManager)
        {

            _mediator = scoreManager.SpritesManager.Mediator;
            
            //AddChild(_canvas);
            //MouseFilter = MouseFilterEnum.Stop;
        }


       

        protected override void SubscribeMovie(LingoMovie movie)
        {
            if (_manager == null) return;
            _manager.SpriteListChanged += OnSpritesChanged;
        }

        protected override void UnsubscribeMovie(LingoMovie movie)
        {
            if (_manager == null) return;
            _manager.SpriteListChanged -= OnSpritesChanged;
        }

        private void OnSpritesChanged()
        {
            if (_movie == null || _manager == null) return;
            RedrawAllSprites();

        }
        public override bool HandleMouseEvent(LingoMouseEvent mouseEvent, int mouseFrame)
        {
            //base.HandleMouseEvent(mouseEvent, currentFrame);
            int channel = SpriteNum - 1;
            var sprite = _sprites.FirstOrDefault(x => x.Sprite.BeginFrame <= mouseFrame && mouseFrame <= x.Sprite.EndFrame);
            //Console.WriteLine("Handle frame:" + channel + " x " + mouseFrame + "\t:");
            if (sprite != null)
                Console.WriteLine("Handle frame:" + mouseFrame + "\t:" + sprite.Sprite.Name + "\t:" + sprite.Sprite.BeginFrame + "->" + sprite.Sprite.EndFrame);
            _spritesManager.ScoreManager.HandleMouse(mouseEvent, channel, mouseFrame);
            //QueueRedraw();
            return true;
        }

        protected abstract TSpriteUI CreateUISprite(TSprite sprite, IDirSpritesManager spritesManager);
        protected virtual void OnDoubleClick(int frame, TSpriteUI? sprite) { }
        protected virtual void OnSpriteClicked(TSpriteUI sprite)
        {
            _mediator.RaiseSpriteSelected(sprite.Sprite);
        }

        public override void SetMovie(LingoMovie? movie)
        {
            base.SetMovie(movie);
            _movie = movie;
            _sprites.Clear();
            if (_movie != null)
            {
                if (_manager != null)
                    _manager.SpriteListChanged -= SpriteListChanged;
                _manager = GetManager(_movie);
                _manager.SpriteListChanged += SpriteListChanged;
                RedrawAllSprites();
            }
            UpdateSize();
        }

        private void SpriteListChanged()
        {
            _hasDirtySprites = true;
            MarkDirty();
        }

        private void RedrawAllSprites()
        {
            foreach (var spriteUI in _sprites)
                spriteUI.Dispose();
            
            _sprites.Clear();
            if (_manager == null) return;
            var sprites = _manager.GetAllSpritesByChannel(SpriteNum);
            foreach (var s in sprites)
            {
                var uiSprite = CreateUISprite(s, _spritesManager);
                uiSprite.Channel = this;
                _sprites.Add(uiSprite);
            }
            MarkDirty();
            _hasDirtySprites = false;
        }
        public override DirScoreSprite? GetSpriteAtFrame(int frame)
        {
            return _sprites.FirstOrDefault(s => s.Sprite.BeginFrame <= frame && frame <= s.Sprite.EndFrame);
        }
        public override DirScoreSprite? FindSprite(LingoSprite sprite)
            => _sprites.FirstOrDefault(s => s.Sprite == sprite);
        protected abstract TSpriteManager GetManager(LingoMovie movie);

       
       

        public override void Draw()
        {
            if (_hasDirtySprites)
                RedrawAllSprites();
            _canvas.Clear(LingoColorList.Transparent);

            //if (_owner.SpritePreviewRect.HasValue)
            //{
            //    var rect = _owner.SpritePreviewRect.Value;
            //    DrawRect(rect, new Color(1, 1, 1, 0.25f), filled: true);
            //    DrawRect(rect, new Color(1, 1, 1, 1), filled: false, width: 1);
            //}

            if (_movie == null) return;

            int channelCount = _movie.MaxSpriteChannelCount;

            foreach (var sp in _sprites)
            {
                sp.Draw(_canvas, _gfxValues.FrameWidth, _gfxValues.ChannelHeight);
            }

            int cur = _movie.CurrentFrame - 1;
            if (cur < 0) cur = 0;
            //float barX = _gfxValues.LeftMargin + cur * _gfxValues.FrameWidth + _gfxValues.FrameWidth / 2f;
            //DrawLine(new Vector2(barX, 0), new Vector2(barX, channelCount * _gfxValues.ChannelHeight), Colors.Red, 2);

            //if (ShowPreview)
            //{
            //    float px = _gfxValues.LeftMargin + (PreviewBegin - 1) * _gfxValues.FrameWidth;
            //    float pw = (PreviewEnd - PreviewBegin + 1) * _gfxValues.FrameWidth;
            //    float py = PreviewChannel * _gfxValues.ChannelHeight;
            //    DrawRect(new Rect2(px, py, pw, _gfxValues.ChannelHeight), new Color(0, 0, 1, 0.3f));
            //}
            _dirty = false;
        }
    }
    }
