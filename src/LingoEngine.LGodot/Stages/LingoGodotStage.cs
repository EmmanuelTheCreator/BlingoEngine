using AbstUI.LGodot.Bitmaps;
using AbstUI.LGodot.Primitives;
using AbstUI.Primitives;
using Godot;
using LingoEngine.Core;
using LingoEngine.Movies;
using LingoEngine.Stages;

namespace LingoEngine.LGodot.Movies
{
    public partial class LingoGodotStage : Node2D, ILingoFrameworkStage, IDisposable
    {
        private Action<IAbstTexture2D>? _pendingShot;
        private bool _pendingExcludeOverlay;
        private LingoStage _LingoStage;
        private readonly LingoClock _lingoClock;
        private readonly LingoDebugOverlay _overlay;
        private LingoPlayer? _player;
        private bool _f1Down;

        private LingoGodotMovie? _activeMovie;

        private Node2D _spriteLayer = null!;
        private CanvasLayer _transitionLayer = null!;
        private Sprite2D _transitionSprite = null!;
        float ILingoFrameworkStage.Scale { get => base.Scale.X; set => base.Scale = new Vector2(value, value); }
        public LingoStage LingoStage => _LingoStage;
        public int X { get => LingoStage.X; set => LingoStage.X = value; }
        public int Y { get => LingoStage.Y; set => LingoStage.Y = value; }

        public LingoGodotStage(LingoPlayer lingoPlayer)
        {
            _lingoClock = (LingoClock)lingoPlayer.Clock;
            _overlay = new LingoDebugOverlay(new Core.LingoGodotDebugOverlay(this), lingoPlayer);

            _spriteLayer = new Node2D();
            AddChild(_spriteLayer);

            _transitionLayer = new CanvasLayer();
            AddChild(_transitionLayer);
            _transitionSprite = new Sprite2D();
            _transitionLayer.AddChild(_transitionSprite);
            _transitionLayer.Visible = false;
        }

        public override void _Ready()
        {
            _transitionSprite.Position = new Vector2(X + LingoStage.Width / 2, Y + LingoStage.Height/2);
            base._Ready();
        }
        public override void _Process(double delta)
        {
            base._Process(delta);
            _lingoClock.Tick((float)delta);
            if (_player != null)
            {
                _overlay.Update((float)delta);
                bool f1 = _player.Key.KeyPressed((int)Key.F1);
                if (f1 && !_f1Down)
                    _overlay.Toggle();
                _f1Down = f1;
                _overlay.Render();
            }

            // fulfill deferred screenshot after this frame's draw
            if (_pendingShot != null)
                TakePendingScreenshot();
        }

     

        internal void Init(LingoStage lingoInstance, LingoPlayer lingoPlayer)
        {
            _LingoStage = lingoInstance;
            _player = lingoPlayer;
        }


        internal void ShowMovie(LingoGodotMovie lingoGodotMovie)
        {
            var node = lingoGodotMovie.GetNode2D();
            // Avoid adding the same node multiple times which results in an error
            if (node.GetParent() != _spriteLayer)
            {
                _spriteLayer.AddChild(node);
            }
        }

        internal void HideMovie(LingoGodotMovie lingoGodotMovie)
        {
            var node = lingoGodotMovie.GetNode2D();
            if (node.GetParent() == _spriteLayer)
                _spriteLayer.RemoveChild(node);
        }

        public void SetActiveMovie(LingoMovie? lingoMovie)
        {
            if (_activeMovie != null)
                _activeMovie.Hide();
            if (lingoMovie == null)
            {
                _activeMovie = null;
                return;
            }
            if (lingoMovie == null) return;
            var godotMovie = lingoMovie.Framework<LingoGodotMovie>();
            _activeMovie = godotMovie;
            godotMovie.Show();
        }

        internal void SetScale(float scale)
        {
            Scale = new Vector2(scale, scale);
        }

        public void ApplyPropertyChanges()
        {
        }


        public void RequestNextFrameScreenshot(Action<IAbstTexture2D> onCaptured, bool excludeTransitionOverlay = true)
        {
            _pendingShot = onCaptured;
            _pendingExcludeOverlay = excludeTransitionOverlay;
            _tickWait = 0;
        }
        private int _tickWait = 0;
        private void TakePendingScreenshot()
        {
            _tickWait++;
            if (_tickWait < 2)
                return; // wait one frame
            // take shot; avoid flicker by not reassigning textures
            var wrap = GetScreenshot();
            if (_pendingShot != null)
                _pendingShot(wrap);
            _pendingShot = null;
            _transitionLayer.Visible = true;
        }
        public IAbstTexture2D GetScreenshot()
        {
            // hide overlay during grab
            bool was = _transitionLayer.Visible;
            _transitionLayer.Visible = false;
            //_spriteLayer.Visible = true;
            var viewport = _spriteLayer.GetViewport();
            Rect2 vis2 = viewport.GetVisibleRect();
            var texx = viewport.GetTexture().GetImage();
            var region2 = texx.GetRegion(new Rect2I(X, Y, LingoStage.Width, LingoStage.Height));
            ImageTexture tex2 = ImageTexture.CreateFromImage(region2);
            _transitionLayer.Visible = true;
           var wrap2 = new AbstGodotTexture2D(tex2, $"StageShot_{_activeMovie!.CurrentFrame}");

#if DEBUG
            wrap2.DebugWriteToDisk();
#endif
            return wrap2;
        }




        public void ShowTransition(IAbstTexture2D startTexture)
        {
            if (startTexture is AbstGodotTexture2D godotTex)
            {
                // keep the ImageTexture reference, just assign it once
                _transitionSprite.Texture = godotTex.Texture;
                _transitionSprite.RegionEnabled = true;
                _transitionSprite.RegionRect = new Rect2(0, 0, startTexture.Width, startTexture.Height);
                //var wrap2 = new AbstGodotTexture2D(_transitionSprite.Texture, $"StageShot_{_activeMovie!.CurrentFrame}");
                //wrap2.DebugWriteToDiskInc();
                //QueueRedraw();
                //_transitionSprite.QueueRedraw();
            }
            _transitionLayer.Visible = false;
            //_spriteLayer.Visible = false;
        }


        public void UpdateTransitionFrame(IAbstTexture2D texture, ARect targetRect)
        {
            if (texture is AbstGodotTexture2D godotTex &&
                _transitionSprite.Texture is ImageTexture imgTex)
            {
                // reuse existing ImageTexture, update its data
                imgTex.Update(godotTex.Texture.GetImage());
                _transitionSprite.RegionEnabled = true;
                _transitionSprite.RegionRect = targetRect.ToRect2();
                //godotTex.DebugWriteToDiskInc();
            }
        }


        public void HideTransition()
        {
            _transitionSprite.Texture = null;
            _transitionLayer.Visible = false;
        }
    }
}
