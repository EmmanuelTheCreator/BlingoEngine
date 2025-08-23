using Godot;
using AbstUI.Primitives;
using AbstUI.LGodot.Bitmaps;
using AbstUI.LGodot.Primitives;
using LingoEngine.Core;
using LingoEngine.Movies;
using LingoEngine.Stages;

namespace LingoEngine.LGodot.Movies
{
    public partial class LingoGodotStage : Node2D, ILingoFrameworkStage, IDisposable
    {
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

        public IAbstTexture2D GetScreenshot()
        {
            bool wasVisible = _transitionLayer.Visible;
            _transitionLayer.Visible = false;
            var img = GetViewport().GetTexture().GetImage();
            _transitionLayer.Visible = wasVisible;
            var tex = ImageTexture.CreateFromImage(img);
            return new AbstGodotTexture2D(tex);
        }

        public void ShowTransition(IAbstTexture2D startTexture)
        {
            if (startTexture is AbstGodotTexture2D godotTex)
            {
                _transitionSprite.Texture = godotTex.Texture;
                _transitionSprite.RegionEnabled = true;
                _transitionSprite.RegionRect = new Rect2(0, 0, startTexture.Width, startTexture.Height);
            }
            _transitionLayer.Visible = true;
        }

        public void UpdateTransitionFrame(IAbstTexture2D texture, ARect targetRect)
        {
            if (texture is AbstGodotTexture2D godotTex)
            {
                _transitionSprite.Texture = godotTex.Texture;
                _transitionSprite.RegionEnabled = true;
                _transitionSprite.RegionRect = targetRect.ToRect2();
            }
        }

        public void HideTransition()
        {
            _transitionSprite.Texture = null;
            _transitionLayer.Visible = false;
        }
    }
}
