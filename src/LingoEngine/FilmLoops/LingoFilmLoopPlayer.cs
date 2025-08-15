using LingoEngine.Events;
using LingoEngine.Sprites;
using LingoEngine.Animations;
using LingoEngine.Bitmaps;
using LingoEngine.Casts;
using LingoEngine.Primitives;
using LingoEngine.AbstUI.Primitives;

namespace LingoEngine.FilmLoops
{
    /// <summary>
    /// Internal helper that plays a <see cref="LingoFilmLoopMember"/> on a sprite.
    /// It subscribes to sprite events but is not exposed as a behaviour.
    /// </summary>
    public class LingoFilmLoopPlayer : IPlayableActor, ILingoSpritesPlayer
    {
        private readonly ILingoSprite2DLight _sprite;
        private readonly ILingoCastLibsContainer _castLibs;
        private readonly bool _isInnerPlayer;
        private readonly ILingoEventMediator _mediator;
        private readonly List<(LingoFilmLoopMemberSprite Entry, LingoSprite2DVirtual Runtime, LingoFilmLoopPlayer? InnerPlayer)> _layers = new();
        private readonly List<LingoSprite2DVirtual> _activeLayers = new();
        private int _currentFrame;
        public int CurrentFrame => _currentFrame;

        public int FrameCount { get; private set; }

        private LingoFilmLoopMember? FilmLoop => _sprite.Member as LingoFilmLoopMember;

        public ILingoTexture2D? Texture { get; private set; }
        private ILingoTextureUserSubscription? _textureSubscription;
        private List<(ILingoTexture2D? Texture, ILingoTextureUserSubscription? TextureSubscription)> _textures = new();

        internal LingoFilmLoopPlayer(ILingoSprite2DLight sprite, ILingoEventMediator eventMediator, ILingoCastLibsContainer castLibs, bool isInnerPlayer = false)
        {
            _sprite = sprite;
            _castLibs = castLibs;
            _isInnerPlayer = isInnerPlayer;
            _mediator = eventMediator;

        }

        public void BeginSprite()
        {
            _currentFrame = 1;
            SetupLayers();
            ApplyFrame();
            if (!_isInnerPlayer)
                _mediator.Subscribe(this, _sprite.SpriteNum + 6);
        }

        private void SetupLayers()
        {
            _textures.Clear();
            _layers.Clear();
            FrameCount = 0;
            var fl = FilmLoop;
            if (fl == null)
                return;

            FrameCount = fl.FrameCount;
            foreach (var entry in fl.SpriteEntries)
            {
                var runtime = new LingoSprite2DVirtual(_mediator, this, entry, _castLibs);
                var properties = entry.AnimatorProperties.Clone();
                if (!properties.Position.HasFirstKeyFrame())
                    // insert the initial sprite properties as keyframe
                    properties.AddKeyFrame(new LingoKeyFrameSetting(1, new APoint(entry.LocH, entry.LocV), new APoint(entry.Width, entry.Height), entry.Rotation, entry.Blend, entry.Skew, entry.ForeColor, entry.BackColor));
                runtime.GetAnimator(properties);
                ApplyFraming(fl, entry, runtime);
                LingoFilmLoopPlayer? nestedPlayer = null;
                if (entry.Member is LingoFilmLoopMember)
                {
                    nestedPlayer = runtime.GetFilmLoopPlayer();
                    if (nestedPlayer == null)
                    {
                        nestedPlayer = new LingoFilmLoopPlayer(runtime, _mediator, _castLibs, true);
                        runtime.AddActor(nestedPlayer);
                        nestedPlayer.BeginSprite();
                    }
                }
                _layers.Add((entry, runtime, nestedPlayer));
            }
        }



        public void StepFrame()
        {
            var fl = FilmLoop;
            if (fl == null || FrameCount == 0)
                return;

            _currentFrame++;
            if (_currentFrame > FrameCount)
            {
                if (fl.Loop)
                    _currentFrame = 1;
                else
                {
                    _currentFrame = FrameCount;
                    return;
                }
            }
            ApplyFrame();
        }

        public void EndSprite()
        {
            if (!_isInnerPlayer)
                _mediator.Unsubscribe(this);
            foreach (var layer in _layers)
            {
                layer.Runtime.RemoveMe();
            }
            _layers.Clear();
            _textureSubscription?.Release();
            _textureSubscription = null;
            Texture = null;
            var fl = FilmLoop;
            if (fl != null)
                fl.Framework<ILingoFrameworkMemberFilmLoop>().Media = null;
            foreach (var textureT in _textures)
                textureT.TextureSubscription?.Release();
            _textures.Clear();
        }

        private void ApplyFrame()
        {
            var fl = FilmLoop;
            if (fl == null)
                return;
            var frameworkFilmLoop = fl.Framework<ILingoFrameworkMemberFilmLoop>();
            if (_textures.Count >= _currentFrame)
            {
                Texture = _textures[_currentFrame-1].Texture;
            }
            else { 
                _activeLayers.Clear();
                foreach (var (entry, runtime, innerplayer) in _layers)
                {
                    var template = entry;
                    bool active = entry.BeginFrame <= _currentFrame && entry.EndFrame >= _currentFrame;
                    if (!active)
                        continue;
                    var animProperties = runtime.GetAnimatorProperties();
                    runtime.SetMember(template.Member);
                    ApplyFraming(fl, template, runtime);

                    float x = template.LocH;
                    float y = template.LocV;
                    float rot = template.Rotation;
                    float skew = template.Skew;
                    float width = runtime.Width;
                    float height = runtime.Height;
                    float blend = template.Blend;
                    var foreColor = template.ForeColor;
                    var backColor = template.BackColor;
                    if (animProperties != null)
                    {
                        if (animProperties.Position.KeyFrames.Count > 1)
                        {
                            var pos = animProperties.Position.GetValue(_currentFrame);
                            x = pos.X;
                            y = pos.Y;
                        }
                        if (animProperties.Size.KeyFrames.Count > 1)
                        {
                            var sz = animProperties.Size.GetValue(_currentFrame);
                            width = sz.X;
                            height = sz.Y;
                        }
                        if (animProperties.Rotation.KeyFrames.Count > 1)
                            rot = animProperties.Rotation.GetValue(_currentFrame);
                        if (animProperties.Skew.KeyFrames.Count > 1)
                            skew = animProperties.Skew.GetValue(_currentFrame);
                        if (animProperties.ForegroundColor.KeyFrames.Count > 1)
                            foreColor = animProperties.ForegroundColor.GetValue(_currentFrame);
                        if (animProperties.BackgroundColor.KeyFrames.Count > 1)
                            backColor = animProperties.BackgroundColor.GetValue(_currentFrame);
                        if (animProperties.Blend.KeyFrames.Count > 1)
                            blend = animProperties.Blend.GetValue(_currentFrame);
                    }

                    runtime.LocH = x;
                    runtime.LocV = y;
                    runtime.Rotation = rot;
                    runtime.Skew = skew;
                    runtime.Width = width;
                    runtime.Height = height;
                    runtime.ForeColor = foreColor;
                    runtime.BackColor = backColor;
                    runtime.Blend = blend;

                    if (innerplayer != null)
                        innerplayer.StepFrame();

                    _activeLayers.Add(runtime);
                }

                if (_activeLayers.Count == 0)
                    return;
                Texture = frameworkFilmLoop.ComposeTexture(_sprite, _activeLayers, _currentFrame);
                _textures.Add((Texture, Texture?.AddUser(this)));
                if (Texture == null) return;
            }
        

            
            //_textureSubscription?.Release();
            //_textureSubscription = null;
            //Texture = null;
            //Texture = frameworkFilmLoop.ComposeTexture(_sprite, _activeLayers, _currentFrame);
            //_textureSubscription = Texture.AddUser(this);
            _sprite.UpdateTexture(Texture!);
        }



        private void ApplyFraming(LingoFilmLoopMember fl, LingoFilmLoopMemberSprite template, LingoSprite2DVirtual runtime)
        {
            if (template.Member is not { } member)
                return;

            float desiredW = member.Width;
            float desiredH = member.Height;

            switch (fl.Framing)
            {
                case LingoFilmLoopFraming.Scale:
                    desiredW = _sprite.Width;
                    desiredH = _sprite.Height;
                    break;
                case LingoFilmLoopFraming.Auto:
                    if (member.Width > _sprite.Width || member.Height > _sprite.Height)
                    {
                        desiredW = _sprite.Width;
                        desiredH = _sprite.Height;
                    }
                    break;
                case LingoFilmLoopFraming.Crop:
                default:
                    desiredW = Math.Min(member.Width, _sprite.Width);
                    desiredH = Math.Min(member.Height, _sprite.Height);
                    break;
            }

            runtime.Width = desiredW;
            runtime.Height = desiredH;
        }

        public int GetMaxLocZ() => FilmLoop != null ? FilmLoop.SpriteEntries.Max(x => x.SpriteNum) : 1;


        public ARect GetBoundingBoxForFrame(int frame)
        {
            var fl = FilmLoop;
            return fl == null ? new ARect() : fl.SpriteEntries.GetBoundingBoxForFrame(frame);
        }

        public ARect GetBoundingBox()
        {
            var fl = FilmLoop;
            return fl == null ? new ARect() : fl.SpriteEntries.GetBoundingBox();
        }


        ///// <summary>
        ///// Adds a sprite to the film loop timeline.
        ///// </summary>
        ///// <param name="channel">Sprite channel inside the film loop.</param>
        ///// <param name="beginFrame">First frame on which the sprite is shown.</param>
        ///// <param name="endFrame">Last frame on which the sprite is shown.</param>
        ///// <param name="sprite">The sprite to add.</param>
        //public void AddSprite(ILingoEventMediator eventMediator, ILingoSpritesPlayer spritesPlayer, int channel, int beginFrame, int endFrame, LingoFilmLoopMemberSprite sprite)
        //    => AddSprite(channel, beginFrame, endFrame, sprite);
    }
}
