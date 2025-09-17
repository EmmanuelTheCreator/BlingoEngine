using LingoEngine.Inputs;
using LingoEngine.Members;
using LingoEngine.Movies;

namespace LingoEngine.Sprites
{
    public class LingoSprite2DManager : LingoSpriteManager<LingoSprite2D>, ILingoSpriteManager<LingoSprite2D>
    {
        //protected readonly Dictionary<int, LingoSprite2D> _frameSpriteBehaviors = new();
        protected LingoStageMouse _lingoMouse;
        
        internal LingoSprite2DManager(LingoMovie movie, LingoMovieEnvironment environment) : base(LingoSprite2D.SpriteNumOffset, movie, environment)
        {
            _lingoMouse = (LingoStageMouse)environment.Mouse;
        }
        internal void SetMouse(LingoStageMouse mouse)
        {
            _lingoMouse = mouse;
        }

        public LingoSprite2D Add(int num, int begin, int end, Action<LingoSprite2D>? configure = null)
         => AddSprite(num, c =>
         {
             c.BeginFrame = begin;
             c.EndFrame = end;
             c.LocZ = num;
             configure?.Invoke(c);
             if (c.Puppet)
                 _newPuppetSprites.Add(c);
         });
        internal LingoSprite2D AddSprite(int num, int begin, int end, float x, float y, Action<LingoSprite2D>? configure = null)
           => AddSprite(num, c =>
           {
               c.BeginFrame = begin;
               c.EndFrame = end;
               c.LocH = x;
               c.LocV = y;
               c.LocZ = num;
               configure?.Invoke(c);
               if (c.Puppet)
                   _newPuppetSprites.Add(c);
           });

        protected override LingoSprite? OnAdd(int spriteNum, int begin, int end, ILingoMember? member)
        {
            var sprite = AddSprite(spriteNum, begin, end, 0, 0, null);
            if (member != null)
                sprite.SetMember(member);
            return sprite;
        }
        protected override LingoSprite2D OnCreateSprite(LingoMovie movie, Action<LingoSprite2D> onRemove)
        {
            var sprite = _environment.Factory.CreateSprite2D(_movie, onRemove);
            return sprite;
        }
        protected override void SpriteJustCreated(LingoSprite2D sprite)
        {
            sprite.LocZ = sprite.SpriteNum;
        }
        internal void ChangeSpriteChannel(LingoSprite2D sprite, int newChannel)
        {
            if (sprite.SpriteNum - 1 == newChannel)
                return;

            int oldChannel = sprite.SpriteNum - 1;
            // _activeSpritesOrdered isn't changing
            _activeSprites.Remove(sprite.SpriteNum);
            _spriteChannels[oldChannel].RemoveSprite();

            var spriteTyped = sprite;
            spriteTyped.ChangeSpriteNumIKnowWhatImDoingOnlyInternal(newChannel + 1);
            _spriteChannels[newChannel].SetSprite(spriteTyped);
            _activeSprites[sprite.SpriteNum] = spriteTyped;

            RaiseSpriteListChanged(sprite.SpriteNum + SpriteNumChannelOffset);
        }


        protected override void SpriteEntered(LingoSprite2D sprite)
        {
            _spriteChannels[sprite.SpriteNum].SetSprite(sprite);
        }
        protected override void SpriteExited(LingoSprite2D sprite)
        {
            _spriteChannels[sprite.SpriteNum].RemoveSprite();
        }

        protected override void OnBeginSprite(LingoSprite2D sprite)
        {
            sprite.FrameworkObj.Show();

            if (!_lingoMouse.IsSubscribed(sprite))
                _lingoMouse.Subscribe(sprite);
            base.OnBeginSprite(sprite);
        }

        protected override void OnPrepareEndSprite(LingoSprite2D sprite)
        {
            base.OnPrepareEndSprite(sprite);

            if (_lingoMouse.IsSubscribed(sprite))
                _lingoMouse.Unsubscribe(sprite);
        }


        List<LingoMember> _changedMembers = new List<LingoMember>();
        internal void PreStepFrame()
        {
            _changedMembers.Clear();
            foreach (var sprite in _activeSpritesOrdered)
            {
                if (sprite.IsActive)
                {
                    var changedMember = sprite.DoPreStepFrame();
                    if (changedMember != null)
                        _changedMembers.Add(changedMember);
                }
            }
            foreach (var item in _changedMembers)
                item.ChangesHasBeenApplied();
        }


        internal ILingoSpriteChannel Channel(int channelNumber) => _spriteChannels[channelNumber];
        internal ILingoSpriteChannel GetActiveSprite(int number) => _spriteChannels[number];


        #region SendSprite
        internal void SendSprite(int spriteNumber, Action<ILingoSpriteChannel> actionOnSprite)
        {
            var sprite = _spriteChannels.Values.FirstOrDefault(x => x.Number == spriteNumber);
            if (sprite == null) return;
            actionOnSprite(sprite);
        }

        internal void SendAllSprites(Action<ILingoSpriteChannel> actionOnSprite)
        {
            foreach (var channel in _spriteChannels.Values)
            {
                if (channel.Sprite != null)
                    actionOnSprite(channel);
            }
        }

        internal void SendAllSprites<T>(Action<T> actionOnSprite) where T : LingoSpriteBehavior
        {
            foreach (var sprite in _activeSpritesOrdered)
                sprite.CallBehavior(actionOnSprite);
        }

        internal IEnumerable<TResult?> SendAllSprites<T, TResult>(Func<T, TResult> actionOnSprite) where T : LingoSpriteBehavior
        {
            foreach (var sprite in _activeSpritesOrdered)
                yield return sprite.CallBehavior(actionOnSprite);
        }



        internal bool TrySendSprite<T>(int spriteNumber, Action<T> actionOnSpriteBehaviour)
            where T : ILingoSpriteBehavior
            => TryCallActiveSprite(spriteNumber, s => s.CallBehavior(actionOnSpriteBehaviour));
        internal void SendSprite<T>(int spriteNumber, Action<T> actionOnSpriteBehaviour)
            where T : ILingoSpriteBehavior
            => CallActiveSprite(spriteNumber, s => s.CallBehavior(actionOnSpriteBehaviour));

        internal TResult? SendSprite<T, TResult>(int spriteNumber, Func<T, TResult> actionOnSpriteBehaviour)
            where T : ILingoSpriteBehavior
            => CallActiveSprite(spriteNumber, s => s.CallBehavior(actionOnSpriteBehaviour));

        internal void SendSprite(string name, Action<ILingoSpriteChannel> actionOnSprite)
        {
            var sprite = _spriteChannels.Values.FirstOrDefault(x => x.Name == name);
            if (sprite == null) return;
            actionOnSprite(sprite);
        }
        #endregion


        internal void SetSpriteMember(int number, string memberName) => CallActiveSprite(number, s => s.SetMember(memberName));
        internal void SetSpriteMember(int number, int memberNumber) => CallActiveSprite(number, s => s.SetMember(memberNumber));


        internal bool RollOver(int spriteNumber)
        {
            if (!_activeSprites.TryGetValue(spriteNumber, out var sprite))
                return false;
            return sprite.IsMouseInsideBoundingBox(_lingoMouse);
        }

        internal int RollOver()
        {
            var sprite = GetSpriteUnderMouse();
            return sprite?.SpriteNum ?? 0;
        }

        internal int ConstrainH(int spriteNumber, int pos)
        {
            if (!_activeSprites.TryGetValue(spriteNumber, out var sprite))
                return pos;
            return (int)MathCompat.Clamp(pos, sprite.Left, sprite.Right);
        }

        internal int ConstrainV(int spriteNumber, int pos)
        {
            if (!_activeSprites.TryGetValue(spriteNumber, out var sprite))
                return pos;
            return (int)MathCompat.Clamp(pos, sprite.Top, sprite.Bottom);
        }

        internal LingoSprite2D? GetSpriteUnderMouse(bool skipLockedSprites = false)
            => GetSpritesAtPoint(_lingoMouse.MouseH, _lingoMouse.MouseV, skipLockedSprites).FirstOrDefault();

        internal IEnumerable<LingoSprite2D> GetSpritesAtPoint(float x, float y, bool skipLockedSprites = false)
        {
            var matches = new List<LingoSprite2D>();
            foreach (var sprite in _activeSpritesOrdered)
            {
                if (skipLockedSprites && sprite.Lock) continue;
                if (sprite.SpriteChannel != null && !sprite.SpriteChannel.Visibility) continue;
                if (sprite.IsPointInsideBoundingBox(x, y))
                    matches.Add(sprite);
            }

            return matches
                .OrderByDescending(s => s.LocH)
                .ThenByDescending(s => s.SpriteNum);
        }

        internal LingoSprite2D? GetSpriteAtPoint(float x, float y, bool skipLockedSprites = false)
            => GetSpritesAtPoint(x, y, skipLockedSprites).FirstOrDefault();

        internal int GetMaxLocZ() => _activeSpritesOrdered.Max(x => x.LocZ);

       
    }
}
