using AbstUI.LGodot.Bitmaps;
using AbstUI.LGodot.Primitives;
using AbstUI.Primitives;
using Godot;
using LingoEngine.Core;
using LingoEngine.Movies;
using LingoEngine.Stages;
using static Godot.OpenXRCompositionLayer;

namespace LingoEngine.LGodot.Movies
{
    public partial class LingoGodotStage : Node2D, ILingoFrameworkStage, IDisposable
    {
        private Action<IAbstTexture2D>? _pendingShot;
        private bool _shotArmed;
        private bool _isDrawingForShot = false;
        private LingoStage _lingoStage = null!;
        private readonly LingoClock _lingoClock;
        private readonly LingoDebugOverlay _overlay;
        private readonly ColorRect _bg;
        private LingoPlayer? _player;
        private bool _f1Down;

        private LingoGodotMovie? _activeMovie;
        private SubViewport _stageSV = null!;
        private SubViewportContainer _stageSVC = null!;
        private Node2D _stageRoot = null!;
        private Node2D _spriteLayer = null!;
        private Sprite2D _transitionStartSprite = null!;
        private Sprite2D _transitionSprite = null!;
        float ILingoFrameworkStage.Scale { get => base.Scale.X; set => base.Scale = new Vector2(value, value); }
        public LingoStage LingoStage => _lingoStage;
        public int X { get => LingoStage.X; set => LingoStage.X = value; }
        public int Y { get => LingoStage.Y; set => LingoStage.Y = value; }

        public LingoGodotStage(LingoPlayer lingoPlayer)
        {
            _lingoClock = (LingoClock)lingoPlayer.Clock;
            _overlay = new LingoDebugOverlay(new Core.LingoGodotDebugOverlay(this), lingoPlayer);


            _stageSV = new SubViewport
            {
                Disable3D = true,
                TransparentBg = false,
                RenderTargetUpdateMode = SubViewport.UpdateMode.Always,
                RenderTargetClearMode = SubViewport.ClearMode.Always,
            };

            _bg = new ColorRect
            {
                Color = Colors.Black,
                Size = new Vector2(300, 300)
            };
            _stageSVC = new SubViewportContainer { Stretch = true, Name = "StageView" };
            AddChild(_stageSVC);
            // mount SubViewport inside the container
            _stageSVC.AddChild(_stageSV);

            _stageSV.AddChild(_bg);
            _stageRoot = new Node2D { Name = "StageRoot" };
            _stageSV.AddChild(_stageRoot);

            _spriteLayer = new Node2D { Name = "SpriteLayer" };
            _stageRoot.AddChild(_spriteLayer);

            _transitionStartSprite = new Sprite2D { Visible = false, ZIndex = 1000 };
            AddChild(_transitionStartSprite);
            _transitionSprite = new Sprite2D { Visible = false, ZIndex = 1001 };
            AddChild(_transitionSprite);
        }

        public override void _Ready()
        {

            base._Ready();
        }
        public override void _Process(double delta)
        {
            base._Process(delta);
            _lingoClock.Tick((float)delta);
            if (_lingoStage.IsDirty)
                _bg.Color = _lingoStage.BackgroundColor.ToGodotColor();
            if (_player != null)
            {
                _overlay.Update((float)delta);
                bool f1 = _player.Key.KeyPressed((int)Key.F1);
                if (f1 && !_f1Down)
                    _overlay.Toggle();
                _f1Down = f1;
                _overlay.Render();
            }
        }



        internal void Init(LingoStage lingoInstance, LingoPlayer lingoPlayer)
        {
            _lingoStage = lingoInstance;
            _player = lingoPlayer;
            _bg.Color = lingoInstance.BackgroundColor.ToGodotColor();
            UpdateSize();
        }

        public void UpdateSize()
        {
            _stageSV.Size = new Vector2I(_lingoStage.Width, _lingoStage.Height);
            _stageSVC.CustomMinimumSize = new Vector2(_lingoStage.Width, _lingoStage.Height);
            _bg.Size = new Vector2(_lingoStage.Width, _lingoStage.Height);
            _bg.CustomMinimumSize = new Vector2(_lingoStage.Width, _lingoStage.Height);
            var center = new Vector2(LingoStage.Width / 2f, LingoStage.Height / 2f);
            _transitionStartSprite.Position = center;
            _transitionSprite.Position = center;
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

        public void RequestNextFrameScreenshot(Action<IAbstTexture2D> onCaptured)
        {
            _pendingShot = onCaptured;
            if (!_shotArmed)
            {
                _shotArmed = true;
                RenderingServer.FramePostDraw += OnFramePostDraw_Screenshot; // capture & restore
            }
        }

        private void OnFramePostDraw_Screenshot()
        {
            try
            {
                if (_pendingShot is null) return;
                var shot = GetScreenshot();   // off-screen: no window presentation occurred
                _pendingShot?.Invoke(shot);
            }
            finally
            {
                // one-shot unsubscribe
                RenderingServer.FramePostDraw -= OnFramePostDraw_Screenshot;
                _pendingShot = null;
                _shotArmed = false;
            }
        }


        public IAbstTexture2D GetScreenshot()
        {
            var texx = _stageSV.GetTexture().GetImage();
            ImageTexture tex2 = ImageTexture.CreateFromImage(texx);
            var wrap2 = new AbstGodotTexture2D(tex2, $"StageShot_{_activeMovie!.CurrentFrame}");
#if DEBUG
            wrap2.DebugWriteToDiskInc();
#endif
            return wrap2;
        }




        public void ShowTransition(IAbstTexture2D startTexture)
        {
#if DEBUG
            AbstGodotTexture2D.ResetDebuggerInc();
#endif
            var godotTex = (AbstGodotTexture2D)startTexture;
            _transitionStartSprite.Texture = godotTex.Texture;
            _transitionStartSprite.Position = new Vector2(startTexture.Width / 2f, startTexture.Height / 2f);
            _transitionStartSprite.Visible = true;

            var img = godotTex.Texture.GetImage();
            var overlayTex = ImageTexture.CreateFromImage(img);
            _transitionSprite.Texture = overlayTex;
            _transitionSprite.RegionEnabled = true;
            _transitionSprite.RegionRect = new Rect2(0, 0, startTexture.Width, startTexture.Height);
            _transitionSprite.Position = new Vector2(startTexture.Width / 2f, startTexture.Height / 2f);
            _transitionSprite.Visible = false;
        }


        public void UpdateTransitionFrame(IAbstTexture2D texture, ARect targetRect)
        {
            var godotTex = (AbstGodotTexture2D)texture;
            if (_transitionSprite.Texture is ImageTexture imgTex)
            {
                // reuse existing ImageTexture, update its data
                imgTex.Update(godotTex.Texture.GetImage());
                _transitionSprite.RegionRect = targetRect.ToRect2();
                _transitionSprite.Position = new Vector2(targetRect.Left + targetRect.Width / 2f,
                    targetRect.Top + targetRect.Height / 2f);
                _transitionSprite.Visible = true;
                //godotTex.DebugWriteToDiskInc();
            }
        }


        public void HideTransition()
        {
            _transitionStartSprite.Texture = null;
            _transitionStartSprite.Visible = false;
            _transitionSprite.Texture = null;
            _transitionSprite.Visible = false;
        }
    }
}
