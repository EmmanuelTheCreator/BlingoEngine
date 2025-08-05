using LingoEngine.Movies;

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
        event Action<int>? SpriteListChanged;
        IEnumerable<TSprite> GetAllSprites();
        IEnumerable<TSprite> GetAllSpritesByChannel(int channel);
        IEnumerable<TSprite> GetAllSpritesBySpriteNumAndChannel(int spriteNumAndChannel);
        
        void MoveSprite(TSprite sprite, int newFrame);
    }



    public abstract class LingoSpriteManager : ILingoSpriteManager
    {
        protected readonly LingoMovieEnvironment _environment;
        protected readonly LingoMovie _movie;
        protected readonly List<int> _mutedSprites = new List<int>();

        protected int _maxSpriteNum = 0;
        protected int _maxSpriteChannelCount;

        public abstract int MaxSpriteChannelCount { get; set; }
        public abstract int SpriteTotalCount { get; }
        public abstract int SpriteMaxNumber { get; }

        protected LingoSpriteManager(LingoMovie movie, LingoMovieEnvironment environment)
        {
            _movie = movie;
            _environment = environment;
        }

        public abstract void MoveSprite(int number, int newFrame);
        public abstract void MuteChannel(int channel, bool state);

        internal abstract void UpdateActiveSprites(int currentFrame, int lastFrame);
        internal abstract void BeginSprites();
        //internal abstract void EndSprites();
    }




    public abstract class LingoSpriteManager<TSprite> : LingoSpriteManager
        where TSprite : LingoSprite
    {
       
        protected readonly Dictionary<int, LingoSpriteChannel> _spriteChannels = new();
        protected readonly Dictionary<string, TSprite> _spritesByName = new();
        protected readonly List<TSprite> _allTimeSprites = new();

        protected readonly Dictionary<int, TSprite> _activeSprites = new();
        protected readonly List<TSprite> _enteredSprites = new();
        protected readonly List<TSprite> _exitedSprites = new();

        internal List<TSprite> AllTimeSprites => _allTimeSprites;
       

        public override int MaxSpriteChannelCount
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

        public override int SpriteTotalCount => _activeSprites.Count;
        public override int SpriteMaxNumber => _activeSprites.Keys.DefaultIfEmpty(0).Max();

        public int SpriteNumChannelOffset { get; }

        public event Action<int>? SpriteListChanged;
        protected void RaiseSpriteListChanged(int spritenumChannel) => SpriteListChanged?.Invoke(spritenumChannel);


        protected LingoSpriteManager(int spritenumChannelOffset, LingoMovie movie, LingoMovieEnvironment environment)
           : base(movie, environment)
        {
            SpriteNumChannelOffset = spritenumChannelOffset;
        }

        public IEnumerable<TSprite> GetAllSprites() => _allTimeSprites.ToArray();
        public IEnumerable<TSprite> GetAllSpritesByChannel(int spriteNum) => _allTimeSprites.Where(x => x.SpriteNum == spriteNum).ToArray();
        public IEnumerable<TSprite> GetAllSpritesBySpriteNumAndChannel(int spriteNumAndChannel) => _allTimeSprites.Where(x => x.SpriteNumWithChannel == spriteNumAndChannel).ToArray();

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
                if (index > -1)
                {
                    _allTimeSprites.RemoveAt(index);
                    _spritesByName.Remove(name);
                    RaiseSpriteListChanged(s.SpriteNumWithChannel);
                }
            });
            sprite.Init(num, name);
            _allTimeSprites.Add(sprite);
            if (!_spritesByName.ContainsKey(name))
                _spritesByName.Add(name, sprite);
            if (num > _maxSpriteNum)
                _maxSpriteNum = num;
            SpriteJustCreated(sprite);

            configure?.Invoke(sprite);
            RaiseSpriteListChanged(sprite.SpriteNumWithChannel);
            return sprite;
        }
        protected abstract TSprite OnCreateSprite(LingoMovie movie, Action<TSprite> onRemove);

        protected virtual void SpriteJustCreated(TSprite sprite)
        {

        }

        public override void MoveSprite(int number, int newFrame)
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
            RaiseSpriteListChanged(sprite.SpriteNumWithChannel);
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
        internal override void UpdateActiveSprites(int currentFrame, int lastFrame)
        {
            _enteredSprites.Clear();
            _exitedSprites.Clear();

            foreach (var sprite in _allTimeSprites)
            {
                if (sprite == null) continue;
                sprite.IsActive = sprite.BeginFrame <= currentFrame && sprite.EndFrame >= currentFrame;

                bool wasActive = sprite.BeginFrame <= lastFrame && sprite.EndFrame >= lastFrame;
                bool isActive = sprite.IsActive;

                if (!wasActive && isActive)
                {
                    _enteredSprites.Add(sprite);
                    if (_activeSprites.TryGetValue(sprite.SpriteNum, out var existingSprite))
                        throw new Exception($"Operlapping sprites:{existingSprite.Name} and {sprite.Name}");
                    //_spriteChannels[sprite.SpriteNum].SetSprite(sprite);
                    _activeSprites.Add(sprite.SpriteNum, sprite);
                }
                else if (wasActive && !isActive)
                {
                    // need to be done early to be able to create new active sprite on same spritenum
                    //_spriteChannels[sprite.SpriteNum].RemoveSprite();
                    _activeSprites.Remove(sprite.SpriteNum);
                    _exitedSprites.Add(sprite);
                    sprite.DoEndSprite();
                }
            }
        }


        internal override void BeginSprites()
        {
            foreach (var sprite in _enteredSprites)
                sprite.DoBeginSprite();
        }
        

        //internal override void EndSprites()
        //{
        //    // Needs to be done earlier when just changing frame
        //}

        public override void MuteChannel(int channel,bool state)
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
