using LingoEngine.Events;
using LingoEngine.Movies;
using LingoEngine.Sprites;
using LingoEngine.Animations;
using System;
using System.Linq;
using System.Collections.Generic;

namespace LingoEngine.FilmLoops
{
    /// <summary>
    /// Internal helper that plays a <see cref="LingoFilmLoopMember"/> on a sprite.
    /// It subscribes to sprite events but is not exposed as a behaviour.
    /// </summary>
    public class LingoFilmLoopPlayer : IPlayableActor
    {
        private readonly LingoSprite2D _sprite;
        private readonly ILingoEventMediator _mediator;
        private readonly ILingoMovieEnvironment _env;
        private readonly List<(LingoFilmLoop.SpriteEntry Entry, LingoSprite2D Runtime)> _layers = new();
        private int _currentFrame;

        internal LingoFilmLoopPlayer(LingoSprite2D sprite, ILingoMovieEnvironment env)
        {
            _sprite = sprite;
            _env = env;
            _mediator = env.Events;
            _mediator.Subscribe(this, sprite.SpriteNum + 6);
        }

        private LingoFilmLoopMember? FilmLoop => _sprite.Member as LingoFilmLoopMember;

        public void BeginSprite()
        {
            _currentFrame = 1;
            SetupLayers();
            _sprite.Visibility = false;
            ApplyFrame();
        }

        public void StepFrame()
        {
            var fl = FilmLoop;
            if (fl == null || fl.FilmLoop.FrameCount == 0)
                return;

            _currentFrame++;
            if (_currentFrame > fl.FilmLoop.FrameCount)
            {
                if (fl.Loop)
                    _currentFrame = 1;
                else
                {
                    _currentFrame = fl.FilmLoop.FrameCount;
                    return;
                }
            }
            ApplyFrame();
        }

        public void EndSprite()
        {
            foreach (var layer in _layers)
            {
                layer.Runtime.RemoveMe();
            }
            _layers.Clear();
            _sprite.Visibility = true;
        }

        private void ApplyFrame()
        {
            var fl = FilmLoop;
            if (fl == null)
                return;

            foreach (var (entry, runtime) in _layers)
            {
                var template = entry.Sprite;
                bool active = entry.BeginFrame <= _currentFrame && entry.EndFrame >= _currentFrame;
                if (!active)
                {
                    runtime.FrameworkObj.Hide();
                    continue;
                }

                runtime.FrameworkObj.Show();
                runtime.SetMember(template.Member);
                ApplyFraming(fl, template, runtime);

                var animator = template.CallActor<LingoSpriteAnimator, LingoSpriteAnimator>(a => a);
                float x = template.LocH;
                float y = template.LocV;
                float rot = template.Rotation;
                float skew = template.Skew;
                if (animator != null)
                {
                    if (animator.Position.KeyFrames.Count > 0)
                    {
                        var pos = animator.Position.GetValue(_currentFrame);
                        x = pos.X;
                        y = pos.Y;
                    }
                    if (animator.Rotation.KeyFrames.Count > 0)
                        rot = animator.Rotation.GetValue(_currentFrame);
                    if (animator.Skew.KeyFrames.Count > 0)
                        skew = animator.Skew.GetValue(_currentFrame);
                }

                runtime.LocH = _sprite.LocH + x;
                runtime.LocV = _sprite.LocV + y;
                runtime.Rotation = rot;
                runtime.Skew = skew;
            }
        }

        private void SetupLayers()
        {
            _layers.Clear();
            var fl = FilmLoop;
            if (fl == null)
                return;
            int index = 0;
            foreach (var entry in fl.FilmLoop.SpriteEntries)
            {
                var tmpl = entry.Sprite;
                var rt = _env.Factory.CreateSprite<LingoSprite2D>((LingoMovie)_env.Movie, _ => { });
                rt.Init(1000 + (++index), "filmLoopLayer" + index);
                rt.SetMember(tmpl.Member);
                rt.LocZ = tmpl.LocZ;
                rt.Ink = tmpl.Ink;
                rt.FrameworkObj.Show();
                ApplyFraming(fl, tmpl, rt);
                _layers.Add((entry, rt));
            }
        }

        private void ApplyFraming(LingoFilmLoopMember fl, LingoSprite2D template, LingoSprite2D runtime)
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
    }
}
