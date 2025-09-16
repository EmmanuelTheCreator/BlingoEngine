using Godot;
using AbstUI.Primitives;
using LingoEngine.LGodot.Sprites;
using LingoEngine.Movies;
using LingoEngine.Sprites;

namespace LingoEngine.LGodot.Movies
{
    public partial class LingoGodotMovie : ILingoFrameworkMovie, IDisposable
    {
        private Node2D _movieNode2D;
        private LingoMovie _lingoMovie;
        private LingoGodotStage _stage;
        private readonly Action<LingoGodotMovie> _removeMethod;
        private HashSet<LingoGodotSprite2D> _drawnSprites = new();
        private HashSet<LingoGodotSprite2D> _allSprites = new();

        public int CurrentFrame => _lingoMovie.CurrentFrame;

        public bool Visibility { get => _movieNode2D.Visible; set => _movieNode2D.Visible = value; }
        public float Width { get => _lingoMovie.Width; set => _lingoMovie.Width = value; }
        public float Height { get => _lingoMovie.Height; set => _lingoMovie.Height = value; }
        public AMargin Margin { get; set; } = AMargin.Zero;
        public string Name { get => _lingoMovie.Name; set => _lingoMovie.Name = value; }
        public int ZIndex { get => _movieNode2D.ZIndex; set => _movieNode2D.ZIndex = value; }

        public object FrameworkNode => _movieNode2D;

        public Node2D GetNode2D() => _movieNode2D;

#pragma warning disable CS8618         
        public LingoGodotMovie(LingoGodotStage stage, LingoMovie lingoInstance, Action<LingoGodotMovie> removeMethod)
#pragma warning restore CS8618 
        {
            _stage = stage;
            _lingoMovie = lingoInstance;
            _removeMethod = removeMethod;

            _movieNode2D = new Node2D();
            _movieNode2D.Name = "MovieRoot";
            //_MovieNode2D.Position = new Vector2(640/2, 480/2);

            stage.ShowMovie(this);
        }

        internal void Show()
        {
            _stage.ShowMovie(this);
        }
        internal void Hide()
        {
            _stage.HideMovie(this);
        }

        public void UpdateStage()
        {
            foreach (var godotSprite in _drawnSprites)
                godotSprite.Update();
        }

        internal void CreateSprite<T>(T lingoSprite) where T : LingoSprite2D
        {
            var godotSprite = new LingoGodotSprite2D(lingoSprite, _movieNode2D, s =>
            {
                // Show Sprite
                _drawnSprites.Add(s);
            }, s =>
            {
                // Hide sprite
                // Remove the sprite from the timeLine
                _drawnSprites.Remove(s);
            }, s =>
            {
                // Dispose
                // Definitly remove sprite from memory when we close the movie
                _drawnSprites.Remove(s);
                _allSprites.Remove(s);
            });
            _allSprites.Add(godotSprite);
        }

        public void RemoveMe()
        {
            _removeMethod(this);
            _movieNode2D.GetParent().RemoveChild(_movieNode2D);
            _movieNode2D.Dispose();
        }
        APoint ILingoFrameworkMovie.GetGlobalMousePosition()
        {
            var pos = _movieNode2D.GetGlobalMousePosition();
            return (pos.X, pos.Y);
        }

        public void Dispose()
        {
            Hide();
            RemoveMe();
        }
    }
}
