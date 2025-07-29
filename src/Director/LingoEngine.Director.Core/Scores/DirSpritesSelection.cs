using LingoEngine.Sprites;

namespace LingoEngine.Director.Core.Scores
{
    public class DirSpritesSelection
    {
        private readonly List<LingoSprite> _sprites = new();
        public IEnumerable<LingoSprite> Sprites => _sprites;

        public event Action? SelectionChanged;
        public event Action? SelectionCleared;

        public void Add(LingoSprite sprite)
        {
            if (sprite == null) return;
            if (!_sprites.Contains(sprite))
            {
                _sprites.Add(sprite);
                SelectionChanged?.Invoke();
            }
        }
        public void Clear()
        {
            _sprites.Clear();
            SelectionChanged?.Invoke();
            SelectionCleared?.Invoke();
        }

        internal void Remove(LingoSprite sprite)
        {
            if (_sprites.Contains(sprite))
            {
                _sprites.Remove(sprite);
                SelectionChanged?.Invoke();

                if (_sprites.Count == 0)
                    SelectionCleared?.Invoke();
            }
        }
        public bool AlreadySelected(LingoSprite sprite) => _sprites.Contains(sprite);
    }
}
