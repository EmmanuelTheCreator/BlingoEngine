using LingoEngine.Inputs;
using LingoEngine.Members;
using LingoEngine.Movies;

namespace LingoEngine.Sprites
{
    internal class LingoSprite2DManager : LingoSpriteManager<LingoSprite2D>
    {
        protected readonly Dictionary<int, LingoSprite2D> _frameSpriteBehaviors = new();
        protected readonly LingoStageMouse _lingoMouse;
        internal LingoSprite2DManager(LingoMovie movie, LingoMovieEnvironment environment) : base(movie, environment)
        {
            _lingoMouse = (LingoStageMouse)environment.Mouse;
        }

        internal LingoSprite2D AddFrameBehavior<TBehaviour>(int frameNumber, Action<TBehaviour>? configureBehaviour = null, Action<LingoSprite2D>? configure = null) where TBehaviour : LingoSpriteBehavior
        {
            var sprite = _environment.Factory.CreateSprite<LingoSprite2D>(_movie, s =>
            {
                _frameSpriteBehaviors.Remove(frameNumber);
                RaiseSpriteListChanged();
            });
            sprite.Init(0, $"FrameSprite_{frameNumber}");
            if (_frameSpriteBehaviors.ContainsKey(frameNumber))
                _frameSpriteBehaviors[frameNumber] = sprite;
            else
                _frameSpriteBehaviors.Add(frameNumber, sprite);
            sprite.BeginFrame = frameNumber;
            sprite.EndFrame = frameNumber;

            var behaviour = sprite.SetBehavior<TBehaviour>();
            configureBehaviour?.Invoke(behaviour);
            configure?.Invoke(sprite);
            RaiseSpriteListChanged();
            return sprite;
        }
        internal LingoSprite2D AddSprite(int num, int begin, int end, float x, float y, Action<LingoSprite2D>? configure = null)
           => AddSprite(num, c =>
           {
               c.BeginFrame = begin;
               c.EndFrame = end;
               c.LocH = x;
               c.LocV = y;
               c.LocZ = num;
               configure?.Invoke(c);
           });
        protected override LingoSprite2D OnCreateSprite(LingoMovie movie, Action<LingoSprite2D> onRemove)
        {
            var sprite = _environment.Factory.CreateSprite<LingoSprite2D>(_movie, onRemove);
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
            _activeSprites.Remove(sprite.SpriteNum);
            _spriteChannels[oldChannel].RemoveSprite();

            var spriteTyped = sprite;
            spriteTyped.ChangeSpriteNumIKnowWhatImDoingOnlyInternal(newChannel + 1);
            _spriteChannels[newChannel].SetSprite(spriteTyped);
            _activeSprites[sprite.SpriteNum] = spriteTyped;

            RaiseSpriteListChanged();
        }

        internal void UpdateActiveSprites(int currentFrame, int lastFrame)
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
                    sprite.FrameworkObj.Show();
                    _enteredSprites.Add(sprite);
                    if (_activeSprites.TryGetValue(sprite.SpriteNum, out var existingSprite))
                        throw new Exception($"Operlapping sprites:{existingSprite.Name}:{existingSprite.Member?.Name} and {sprite.Name}:{sprite.Member?.Name}");
                    _spriteChannels[sprite.SpriteNum].SetSprite(sprite);
                    _activeSprites.Add(sprite.SpriteNum, sprite);
                    if (!_lingoMouse.IsSubscribed(sprite))
                        _lingoMouse.Subscribe(sprite);
                }
                else if (wasActive && !isActive)
                {
                    // need to be done early to be able to create new active sprite on same spritenum
                    _spriteChannels[sprite.SpriteNum].RemoveSprite();
                    if (_lingoMouse.IsSubscribed(sprite))
                        _lingoMouse.Unsubscribe(sprite);
                    _activeSprites.Remove(sprite.SpriteNum);
                    _exitedSprites.Add(sprite);
                    sprite.FrameworkObj.Hide();
                    sprite.DoEndSprite();
                }
            }

            if (_frameSpriteBehaviors.TryGetValue(currentFrame, out var frameSprite))
                _currentFrameSprite = frameSprite;
            else
                _currentFrameSprite = null;
        }

        List<LingoMember> _changedMembers = new List<LingoMember>();
        internal void PreStepFrame()
        {
            _changedMembers.Clear();
            foreach (var sprite in _activeSprites.Values)
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
            var sprite = _spriteChannels.Values.FirstOrDefault(x => x.SpriteNum == spriteNumber);
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
            foreach (var sprite in _activeSprites.Values)
                sprite.CallBehavior(actionOnSprite);
        }

        internal IEnumerable<TResult?> SendAllSprites<T, TResult>(Func<T, TResult> actionOnSprite) where T : LingoSpriteBehavior
        {
            foreach (var sprite in _activeSprites.Values)
                yield return sprite.CallBehavior(actionOnSprite);
        }



        internal void SendSprite<T>(int spriteNumber, Action<T> actionOnSpriteBehaviour)
            where T : LingoSpriteBehavior
            => CallActiveSprite(spriteNumber, s => s.CallBehavior(actionOnSpriteBehaviour));

        internal TResult? SendSprite<T, TResult>(int spriteNumber, Func<T, TResult> actionOnSpriteBehaviour)
            where T : LingoSpriteBehavior
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
            var sprite = _activeSprites[spriteNumber];
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
            return (int)Math.Clamp(pos, sprite.Left, sprite.Right);
        }

        internal int ConstrainV(int spriteNumber, int pos)
        {
            if (!_activeSprites.TryGetValue(spriteNumber, out var sprite))
                return pos;
            return (int)Math.Clamp(pos, sprite.Top, sprite.Bottom);
        }

        internal LingoSprite2D? GetSpriteUnderMouse(bool skipLockedSprites = false)
            => GetSpritesAtPoint(_lingoMouse.MouseH, _lingoMouse.MouseV, skipLockedSprites).FirstOrDefault();

        internal IEnumerable<LingoSprite2D> GetSpritesAtPoint(float x, float y, bool skipLockedSprites = false)
        {
            var matches = new List<LingoSprite2D>();
            foreach (var sprite in _activeSprites.Values)
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
       
        internal int GetMaxLocZ() => _activeSprites.Values.Max(x => x.LocZ);

        internal IReadOnlyDictionary<int, LingoSprite2D> FrameSpriteBehaviors => _frameSpriteBehaviors;

        internal void MoveFrameBehavior(int previousFrame, int newFrame)
        {
            if (previousFrame == newFrame) return;
            if (!_frameSpriteBehaviors.TryGetValue(previousFrame, out var sprite))
                return;

            _frameSpriteBehaviors.Remove(previousFrame);

            if (_frameSpriteBehaviors.TryGetValue(newFrame, out var existing))
                existing.RemoveMe();

            _frameSpriteBehaviors[newFrame] = sprite;
            sprite.BeginFrame = newFrame;
            sprite.EndFrame = newFrame;

            RaiseSpriteListChanged();
        }
    }
}
