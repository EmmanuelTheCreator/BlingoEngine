using LingoEngine.Movies;
using System.Collections.Generic;

namespace LingoEngine.Sprites
{

    public interface ILingoSpriteManager
    {
        int MaxSpriteChannelCount { get; }
        int SpriteTotalCount { get; }
        int SpriteMaxNumber { get; }
        void MoveSprite(int number, int newFrame);
        void MuteChannel(int channel, bool state);
    }
    public interface ILingoSpriteManager<TSprite> : ILingoSpriteManager
    {
        event Action? SpriteListChanged;
        IEnumerable<TSprite> GetAllSprites();
        void MoveSprite(TSprite sprite, int newFrame);
    }
    internal abstract class LingoSpriteManager<TSprite> : ILingoSpriteManager
        where TSprite : LingoSprite
    {
        protected readonly LingoMovieEnvironment _environment;
        protected readonly LingoMovie _movie;
        protected readonly List<int> _mutedSprites = new List<int>();

        protected int _maxSpriteNum = 0;
        protected int _maxSpriteChannelCount;
        protected readonly Dictionary<int, LingoSpriteChannel> _spriteChannels = new();
        protected readonly Dictionary<string, TSprite> _spritesByName = new();
        protected readonly List<TSprite> _allTimeSprites = new();

        protected readonly Dictionary<int, TSprite> _activeSprites = new();
        protected readonly List<TSprite> _enteredSprites = new();
        protected readonly List<TSprite> _exitedSprites = new();
        protected TSprite? _currentFrameSprite;

        internal List<TSprite> AllTimeSprites => _allTimeSprites;
        public event Action? SpriteListChanged;
        protected void RaiseSpriteListChanged() => SpriteListChanged?.Invoke();
        public IEnumerable<TSprite> GetAllSprites() => _allTimeSprites.ToArray();
        internal LingoSpriteManager(LingoMovie movie, LingoMovieEnvironment environment)
        {
            _movie = movie;
            _environment = environment;
        }

        public int MaxSpriteChannelCount
        {
            get => _maxSpriteChannelCount;
            set
            {
                if (value > 0)
                {
                    _maxSpriteChannelCount = value;
                    if (_spriteChannels.Count < _maxSpriteChannelCount)
                    {
                        for (int i = _spriteChannels.Count; i < _maxSpriteChannelCount; i++)
                            _spriteChannels.Add(i, new LingoSpriteChannel(i));
                    }
                }
            }
        }

        public int SpriteTotalCount => _activeSprites.Count;
        public int SpriteMaxNumber => _activeSprites.Keys.DefaultIfEmpty(0).Max();


        internal TSprite AddSprite(string name, Action<TSprite>? configure = null)
        {
            _maxSpriteNum++;
            var num = _maxSpriteNum;
            return AddSprite(num, name, configure);
        }

        internal TSprite AddSprite(int num, Action<TSprite>? configure = null) => AddSprite(num, "Sprite_" + num, configure);

        internal TSprite AddSprite(int num, string name, Action<TSprite>? configure = null)
        {
            var sprite = OnCreateSprite(_movie, s =>
            {
                var index = _allTimeSprites.IndexOf(s);
                _allTimeSprites.RemoveAt(index);
                _spritesByName.Remove(name);
                RaiseSpriteListChanged();
            });
            sprite.Init(num, name);
            _allTimeSprites.Add(sprite);
            if (!_spritesByName.ContainsKey(name))
                _spritesByName.Add(name, sprite);
            if (num > _maxSpriteNum)
                _maxSpriteNum = num;
            SpriteJustCreated(sprite);

            configure?.Invoke(sprite);
            RaiseSpriteListChanged();
            return sprite;
        }
        protected abstract TSprite OnCreateSprite(LingoMovie movie, Action<TSprite> onRemove);

        protected virtual void SpriteJustCreated(TSprite sprite)
        {

        }

        public void MoveSprite(int number, int newFrame)
        {
            if (number <= 0 || number > _movie.MaxSpriteChannelCount)
                return;
            var sprite = _allTimeSprites[number - 1];
            if (sprite == null) return;
            MoveSprite(sprite, newFrame);
        }
        public void MoveSprite(TSprite sprite, int newFrame)
        {
            if (newFrame <= 0 || newFrame > _movie.FrameCount)
                return;
            var duration = sprite.EndFrame - sprite.BeginFrame;
            sprite.BeginFrame = newFrame;
            sprite.EndFrame = newFrame + duration;
            RaiseSpriteListChanged();
        }

        internal bool RemoveSprite(string name)
        {
            if (!_spritesByName.TryGetValue(name, out var sprite))
                return false;
            sprite.RemoveMe();
            return true;
        }

        internal bool TryGetAllTimeSprite(string name, out TSprite? sprite)
        {
            if (_spritesByName.TryGetValue(name, out var sprite1))
            {
                sprite = sprite1;
                return true;
            }
            sprite = null;
            return false;
        }

        internal bool TryGetAllTimeSprite(int number, out TSprite? sprite)
        {
            if (number <= 0 || number > _allTimeSprites.Count)
            {
                sprite = null;
                return false;
            }
            sprite = _allTimeSprites[number - 1];
            return true;
        }
        internal void PuppetSprite(int number, bool isPuppetSprite) => CallActiveSprite(number, sprite => sprite.Puppet = isPuppetSprite);



        protected void CallActiveSprites(Action<TSprite> actionOnAllActiveSprites)
        {
            foreach (var sprite in _activeSprites.Values)
                actionOnAllActiveSprites(sprite);
        }
        protected void CallActiveSprite(int number, Action<TSprite> spriteAction)
        {
            var sprite = _activeSprites[number];
            if (sprite == null) return;
            spriteAction(sprite);
        }
        protected TResult? CallActiveSprite<TResult>(int number, Func<TSprite, TResult?> spriteAction)
        {
            var sprite = _activeSprites[number];
            if (sprite == null) return default;
            return spriteAction(sprite);
        }



        internal void BeginSprites()
        {
            foreach (var sprite in _enteredSprites)
                sprite.DoBeginSprite();

            _currentFrameSprite?.DoBeginSprite();
        }


        internal void EndSprites()
        {
            _currentFrameSprite?.DoEndSprite();
            //foreach (var sprite in _exitedSprites)
            //{
            //    sprite.FrameworkObj.Hide();
            //    sprite.DoEndSprite();

            //}
        }

        public virtual void MuteChannel(int channel,bool state)
        {
            if(state)
            {
                if (!_mutedSprites.Contains(channel))
                    _mutedSprites.Add(channel);
            }
            else
            {
                if (_mutedSprites.Contains(channel))
                    _mutedSprites.Remove(channel);
            }
        }
    }
}
