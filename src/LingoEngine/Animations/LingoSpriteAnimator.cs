using LingoEngine.Primitives;
using LingoEngine.Events;
using LingoEngine.Sprites;

namespace LingoEngine.Animations
{
    public class LingoSpriteAnimator : IPlayableActor
    {
        private LingoSpriteMotionPath _cachedPath = new();
        
        private readonly ILingoSpritesPlayer _spritesPlayer;
        private readonly ILingoSprite2DLight _sprite;
        //private readonly ILingoMovie _movie;
        private readonly ILingoEventMediator _mediator;

        private List<Action<int>> _applyActions = new();

        private LingoSpriteAnimatorProperties _properties;
        public LingoSpriteAnimatorProperties Properties => _properties;




        public LingoSpriteAnimator(ILingoSprite2DLight sprite, ILingoSpritesPlayer spritesPlayer, ILingoEventMediator mediator, LingoSpriteAnimatorProperties? spriteAnimatorProperties = null)
        {
            _sprite = sprite;
            _spritesPlayer = spritesPlayer;
            _mediator = mediator;
            _properties = spriteAnimatorProperties ?? new LingoSpriteAnimatorProperties();
            _mediator.Subscribe(this, sprite.SpriteNum + 6);
        }

        public void SetTweenOptions(bool positionEnabled, bool sizeEnabled, bool rotationEnabled, bool skewEnabled,
            bool foreColorEnabled, bool backColorEnabled, bool blendEnabled,
            float curvature, bool continuousAtEnds, bool smoothSpeed, float easeIn, float easeOut)
        {
            _properties.SetTweenOptions(positionEnabled, sizeEnabled, rotationEnabled, skewEnabled,
                foreColorEnabled, backColorEnabled, blendEnabled,
                curvature, continuousAtEnds, smoothSpeed, easeIn, easeOut);
        }

        
        public void AddKeyFrame(LingoKeyFrameSetting setting) => _properties.AddKeyFrame(setting);
        public void UpdateKeyFrame(LingoKeyFrameSetting setting) => _properties.UpdateKeyFrame(setting);

        internal void AddKeyFrames(params LingoKeyFrameSetting[] keyframes)
        {
            foreach (var item in keyframes)
                AddKeyFrame(item);
            RecalculateCache();
        }

        private void Apply(int frame)
        {
            foreach (var action in _applyActions)
                action.Invoke(frame);
           
        }

        public void BeginSprite()
        {
            EnsureCache();
            Apply(_spritesPlayer.CurrentFrame);
        }

        public void StepFrame()
        {
            Apply(_spritesPlayer.CurrentFrame);
        }

        public void EndSprite()
        {
        }

        internal LingoSpriteMotionPath GetMotionPath(int startFrame, int endFrame)
        {
            EnsureCache();
            var path = new LingoSpriteMotionPath();
            foreach (var f in _cachedPath.Frames)
            {
                if (f.Frame < startFrame || f.Frame > endFrame) continue;
                path.Frames.Add(f);
            }
            return path;
        }

        internal void EnsureCache()
        {
            if (!_properties.CacheIsDirty) return;
            
            RecalculateCache();
        }

        internal void RecalculateCache()
        {
            _cachedPath = new LingoSpriteMotionPath();
            if (_properties.Position.KeyFrames.Count > 0)
            {
                int start = _properties.Position.KeyFrames.First().Frame;
                int end = _properties.Position.KeyFrames.Last().Frame;
                for (int frame = start; frame <= end; frame++)
                {
                    var pos = _properties.Position.GetValue(frame);
                    bool isKey = _properties.Position.KeyFrames.Any(k => k.Frame == frame);
                    _cachedPath.Frames.Add(new LingoSpriteMotionFrame(frame, pos, isKey));
                }
            }
            _applyActions.Clear();
            if (_properties.Position.Options.Enabled && _properties.Position.HasKeyFrames)
                _applyActions.Add(frame => _sprite.Loc = _properties.Position.GetValue(frame));
            if (_properties.Size.Options.Enabled && _properties.Size.HasKeyFrames)
                _applyActions.Add(frame =>
                {
                    var size = _properties.Size.GetValue(frame);
                    _sprite.Width = size.X;
                    _sprite.Height = size.Y;
                });
            if (_properties.Rotation.Options.Enabled && _properties.Rotation.HasKeyFrames)
                _applyActions.Add(frame => _sprite.Rotation = _properties.Rotation.GetValue(frame));
            if (_properties.Skew.Options.Enabled && _properties.Skew.HasKeyFrames)
                _applyActions.Add(frame => _sprite.Skew = _properties.Skew.GetValue(frame));
            if (_properties.ForegroundColor.Options.Enabled && _properties.ForegroundColor.HasKeyFrames)
                _applyActions.Add(frame => _sprite.ForeColor = _properties.ForegroundColor.GetValue(frame));
            if (_properties.BackgroundColor.Options.Enabled && _properties.BackgroundColor.HasKeyFrames)
                _applyActions.Add(frame => _sprite.BackColor = _properties.BackgroundColor.GetValue(frame));
            if (_properties.Blend.Options.Enabled && _properties.Blend.HasKeyFrames)
                _applyActions.Add(frame => _sprite.Blend = _properties.Blend.GetValue(frame));
            _properties.CacheApplied();
            _properties.RequestRecalculatedBoundingBox();// push to recalculate the boundingbox
           
        }


        #region Boundingbox
        public void SpriteRegPointOrSizeChanged() => _properties.RequestRecalculatedBoundingBox();

        public LingoRect GetBoundingBox() => _properties.GetBoundingBoxForFrame(_spritesPlayer.CurrentFrame, _sprite.RegPoint, _sprite.Width, _sprite.Height);


        public LingoRect GetBoundingBoxForFrame(int frame) 
            => _properties.GetBoundingBoxForFrame(frame, _sprite.RegPoint, _sprite.Width, _sprite.Height);

        #endregion


    }
}
