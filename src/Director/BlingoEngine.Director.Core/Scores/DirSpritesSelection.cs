using BlingoEngine.Sprites;

namespace BlingoEngine.Director.Core.Scores
{
    public class DirSpritesSelection
    {
        private readonly List<BlingoSprite> _sprites = new();
        public IEnumerable<BlingoSprite> Sprites => _sprites;

        public event Action? SelectionChanged;
        public event Action? SelectionCleared;

        public void Add(BlingoSprite sprite)
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

        internal void Remove(BlingoSprite sprite)
        {
            if (_sprites.Contains(sprite))
            {
                _sprites.Remove(sprite);
                SelectionChanged?.Invoke();

                if (_sprites.Count == 0)
                    SelectionCleared?.Invoke();
            }
        }
        public bool AlreadySelected(BlingoSprite sprite) => _sprites.Contains(sprite);
    }
}

