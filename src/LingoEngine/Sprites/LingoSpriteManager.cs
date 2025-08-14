using LingoEngine.Members;
using LingoEngine.Movies;

namespace LingoEngine.Sprites
{

    public interface ILingoSpriteManager
    {
        int MaxSpriteChannelCount { get; }
        int SpriteTotalCount { get; }
        int SpriteMaxNumber { get; }
        int SpriteNumChannelOffset { get; }
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
        public int SpriteNumChannelOffset { get; protected set; }

        protected LingoSpriteManager(LingoMovie movie, LingoMovieEnvironment environment)
        {
            _movie = movie;
            _environment = environment;
        }

        public abstract void MoveSprite(int number, int newFrame);
        public abstract void MuteChannel(int channel, bool state);

        internal abstract void UpdateActiveSprites(int currentFrame, int lastFrame);
        internal abstract void BeginSprites();
        internal virtual LingoSprite? Add(int spriteNumWithChannel, int begin, int end, ILingoMember? member)
        {
            return OnAdd(spriteNumWithChannel- SpriteNumChannelOffset, begin, end, member);
        }
        protected abstract LingoSprite? OnAdd(int spriteNum, int begin, int end, ILingoMember? member);
        internal abstract void EndSprites();
    }




    public abstract class LingoSpriteManager<TSprite> : LingoSpriteManager
        where TSprite : LingoSprite
    {
       
        protected readonly Dictionary<int, LingoSpriteChannel> _spriteChannels = new();
        protected readonly Dictionary<string, TSprite> _spritesByName = new();
        protected readonly List<TSprite> _allTimeSprites = new();

        protected readonly Dictionary<int, TSprite> _activeSprites = new();
        protected readonly List<TSprite> _activeSpritesOrdered = new();
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
                            _spriteChannels.Add(i, new LingoSpriteChannel(i,_movie));
                    }
                }
            }
        }

        public override int SpriteTotalCount => _activeSprites.Count;
        public override int SpriteMaxNumber => _activeSprites.Keys.DefaultIfEmpty(0).Max();

        

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
        internal bool RemoveSprite(LingoSprite2D sprite)
        {
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


        protected void CallActiveSprites(Action<TSprite> actionOnAllActiveSprites)
        {
            foreach (var sprite in _activeSpritesOrdered)
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

            foreach (var sprite in _activeSpritesOrdered.ToArray()) // make a copy of the array
            {
                bool stillActive = sprite.BeginFrame <= currentFrame && sprite.EndFrame >= currentFrame;
                if (!stillActive)
                {
                    _exitedSprites.Add(sprite);
                    _activeSprites.Remove(sprite.SpriteNum);
                    _activeSpritesOrdered.Remove(sprite);
                    SpriteExited(sprite);
                }
            }
            foreach (var sprite in _allTimeSprites)
            {
                if (sprite.IsActive) continue;
                var isActive = sprite.BeginFrame <= currentFrame && sprite.EndFrame >= currentFrame;
                if (isActive)
                {
                    sprite.IsActive = true;
                    _enteredSprites.Add(sprite);
                    if (_activeSprites.TryGetValue(sprite.SpriteNum, out var existingSprite))
                        //throw new Exception($"Operlapping sprites:{existingSprite.Name}:{existingSprite.Member?.Name} and {sprite.Name}:{sprite.Member?.Name}");
                        throw new Exception($"Operlapping sprites:{existingSprite.SpriteNum}) {existingSprite.Name} and {sprite.SpriteNum}) {sprite.Name}");
                    _activeSprites.Add(sprite.SpriteNum, sprite);
                    _activeSpritesOrdered.Add(sprite);
                    SpriteEntered(sprite);

                }
            }
            foreach (var sprite in _exitedSprites)
                sprite.IsActive = false;
        }

        protected virtual void SpriteEntered(TSprite sprite)
        {
            //_spriteChannels[sprite.SpriteNum].SetSprite(sprite);
        }
        protected virtual void SpriteExited(TSprite sprite)
        {
            //_spriteChannels[sprite.SpriteNum].RemoveSprite();
        }


        internal override void BeginSprites()
        {
            foreach (var sprite in _enteredSprites)
                OnBeginSprite(sprite);
        }
        protected virtual void OnBeginSprite(TSprite sprite) => sprite.DoBeginSprite();

        internal override void EndSprites()
        {
            foreach (var sprite in _exitedSprites)
                OnEndSprite(sprite);
        }
        protected virtual void OnEndSprite(TSprite sprite) => sprite.DoEndSprite();



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

        public int GetNextSpriteStart(int channel, int frame)
        {
            int next = int.MaxValue;
            foreach (var sp in _allTimeSprites)
            {
                if (sp.SpriteNum - 1 == channel && sp.BeginFrame > frame)
                    next = Math.Min(next, sp.BeginFrame);
            }
            return next == int.MaxValue ? -1 : next;
        }

        public int GetPrevSpriteEnd(int channel, int frame)
        {
            int prev = -1;
            foreach (var sp in _allTimeSprites)
            {
                if (sp.SpriteNum - 1 == channel && sp.EndFrame < frame)
                    prev = Math.Max(prev, sp.EndFrame);
            }
            return prev;
        }
    }
}
