﻿using LingoEngine.Events;
using LingoEngine.Inputs;
using LingoEngine.Primitives;
using LingoEngine.Animations;
using LingoEngine.Members;
using LingoEngine.Casts;
using LingoEngine.Movies;
using LingoEngine.FilmLoops;
using LingoEngine.Sprites.Events;
using LingoEngine.Bitmaps;

namespace LingoEngine.Sprites
{


    public class LingoSprite : ILingoSprite, ILingoMouseEventHandler
    {
        private readonly ILingoMovieEnvironment _environment;
        private readonly LingoEventMediator _eventMediator;
        private readonly List<LingoSpriteBehavior> _behaviors = new();
        private readonly List<object> _spriteActors = new();

        public IReadOnlyList<LingoSpriteBehavior> Behaviors => _behaviors;

        private ILingoFrameworkSprite _frameworkSprite;
        private bool isMouseInside = false;
        private bool isDragging = false;
        private bool isDraggable = false;  // A flag to control dragging behavior
        private bool _lock = false;
        private LingoMember? _Member;
        private Action<LingoSprite>? _onRemoveMe;
        private bool _isFocus = false;
        private bool _flipH;
        private bool _flipV;
        private int _cursor;
        private int? _previousCursor;
        private int _constraint;
        private bool _directToStage;
        private float _blend = 1f;


        #region Properties
        internal LingoSpriteChannel? SpriteChannel { get; set; }
        public ILingoFrameworkSprite FrameworkObj => _frameworkSprite;
        public T Framework<T>() where T : class, ILingoFrameworkSprite => (T)_frameworkSprite;

        /// <summary>
        /// This represents the puppetsprite controlled by script.
        /// </summary>
        public bool Puppet { get; set; }

        public string Name { get => _frameworkSprite.Name; set => _frameworkSprite.Name = value; }
        public int MemberNum
        {
            get => Member?.NumberInCast ?? 0; 
            set
            {
                if (Member != null)
                    //Member = Member.GetMemberInCastByOffset(value);
                    Member = Member.Cast.Member[value];
            }
        }
        /// <summary>Channel default cast member.</summary>
        public int DisplayMember { get; set; }
        /// <summary>Offset to the property list for behaviors.</summary>
        public int SpritePropertiesOffset { get; set; }
        public int SpriteNum { get; private set; }
        private int _ink;
        public LingoInkType InkType { get => (LingoInkType)_ink; set => Ink = (int)value; }
        public int Ink
        {
            get => _ink;
            set
            {
                _ink = value;
                if (_frameworkSprite != null)
                    _frameworkSprite.Ink = value;
            }
        }
        public bool Visibility
        {
            get => _frameworkSprite.Visibility; set
            {
                if (SpriteChannel != null)
                    SpriteChannel.Visibility = value;
                _frameworkSprite.Visibility = value;
            }
        }
        public bool Hilite { get; set; }
        public bool Linked { get; private set; }
        public bool Loaded { get; private set; }
        public bool MediaReady { get; private set; }
        public float Blend
        {
            get => _blend;
            set
            {
                _blend = value;
                ApplyBlend();
            }
        }
        public float LocH { get => _frameworkSprite.X; set => _frameworkSprite.X = value; }
        public float LocV { get => _frameworkSprite.Y; set => _frameworkSprite.Y = value; }
        public int LocZ { get => _frameworkSprite.ZIndex; set => _frameworkSprite.ZIndex = value; }
        public LingoPoint Loc { get => (_frameworkSprite.X, _frameworkSprite.Y); set => _frameworkSprite.SetPosition(value); }

        public float Rotation { get => _frameworkSprite.Rotation; set => _frameworkSprite.Rotation = value; }
        public float Skew { get => _frameworkSprite.Skew; set => _frameworkSprite.Skew = value; }
        public bool FlipH { get => _flipH; set { _flipH = value; _frameworkSprite.FlipH = value; } }
        public bool FlipV { get => _flipV; set { _flipV = value; _frameworkSprite.FlipV = value; } }
        public float Top { get => Rect.Top; set { var o = GetRegPointOffset(); LocV = value + o.Y + Height / 2f; } }
        public float Bottom { get => Rect.Bottom; set => Top = value - Height; }
        public float Left { get => Rect.Left; set { var o = GetRegPointOffset(); LocH = value + o.X + Width / 2f; } }
        public float Right { get => Rect.Right; set => Left = value - Width; }
        public int Cursor { get => _cursor; set => _cursor = value; }
        public int Constraint { get => _constraint; set => _constraint = value; }
        public bool DirectToStage
        {
            get => _directToStage;
            set
            {
                if (_directToStage == value) return;
                _directToStage = value;
                // Rendering order is handled by the framework implementation,
                // so keep the engine Z index untouched.
                _frameworkSprite.DirectToStage = value;
                ApplyBlend();
            }
        }

        public LingoPoint RegPoint { get; set; }
        public LingoColor ForeColor { get; set; } = LingoColorList.Black;
        public LingoColor BackColor { get; set; } = LingoColorList.White;
        public List<string> ScriptInstanceList { get; private set; } = new();



        public ILingoMember? Member { get => _Member; set => SetMember(value); }
        public LingoCast? Cast { get; private set; }

        public int BeginFrame { get; set; }
        public int EndFrame { get; set; }

        public bool Editable { get; set; }
        public bool Lock
        {
            get => _lock;
            set => _lock = value;
        }
        public bool IsDraggable
        {
            get => isDraggable;
            set => isDraggable = value;
        }

        public LingoColor Color { get; set; }
        
        public LingoRect Rect
        {
            get
            {
                var offset = GetRegPointOffset();
                float left = LocH - offset.X - Width / 2f;
                float top = LocV - offset.Y - Height / 2f;
                return new LingoRect(left, top, left + Width, top + Height);
            }
        }

        public int Size => Media.Length;

        public byte[] Media { get; set; } = new byte[] { };
        public byte[] Thumbnail { get; set; } = new byte[] { };
        public string ModifiedBy { get; set; } = "";

        public float Width { get => _frameworkSprite.Width; set => _frameworkSprite.SetDesiredWidth = value; }
        public float Height { get => _frameworkSprite.Height; set => _frameworkSprite.SetDesiredHeight = value; }
        /// <summary>
        /// Whether this sprite is currently active (i.e., the playhead is within its frame span).
        /// </summary>
        public bool IsActive { get; internal set; }

        #endregion

        private void ApplyBlend()
        {
            _frameworkSprite.Blend = _directToStage ? 1f : _blend;
        }

        private LingoPoint GetRegPointOffset()
        {
            if (_Member is { } member)
            {
                var baseOffset = member.CenterOffsetFromRegPoint();
                if (member.Width != 0 && member.Height != 0)
                {
                    float scaleX = Width / member.Width;
                    float scaleY = Height / member.Height;
                    return new LingoPoint(baseOffset.X * scaleX, baseOffset.Y * scaleY);
                }
                return baseOffset;
            }
            return new LingoPoint();
        }



        // Not used in c#
        // public int ScriptText { get; set; }

#pragma warning disable CS8618
        public LingoSprite(ILingoMovieEnvironment environment)
#pragma warning restore CS8618
        {
            _environment = environment;
            _eventMediator = (LingoEventMediator)_environment.Events;
        }
        public void Init(ILingoFrameworkSprite frameworkSprite)
        {
            _frameworkSprite = frameworkSprite;
            _frameworkSprite.Ink = _ink;
            ApplyBlend();
        }
        internal void Init(int number, string name)
        {
            SpriteNum = number;
            Name = name;
        }

        public ILingoSprite AddBehavior<T>() where T : LingoSpriteBehavior
        {
            SetBehavior<T>();
            return this;
        }
        public T SetBehavior<T>() where T : LingoSpriteBehavior
        {
            var behavior = _environment.Factory.CreateBehavior<T>((LingoMovie)_environment.Movie);
            behavior.SetMe(this);
            _behaviors.Add(behavior);

            return behavior;
        }

        /// <summary>
        /// Adds animation keyframes for this sprite. When invoked for the first time
        /// it lazily creates a <see cref="LingoSpriteAnimator"/> actor and stores it
        /// in the internal sprite actors list.
        /// </summary>
        /// <param name="keyframes">Tuple list containing frame number, X, Y, rotation and skew values.</param>
        public void AddKeyframes(params (int Frame, float X, float Y, float Rotation, float Skew)[] keyframes)
        {
            if (keyframes == null || keyframes.Length == 0)
                return;

            var animator = _spriteActors.OfType<LingoSpriteAnimator>().FirstOrDefault();
            if (animator == null)
            {
                animator = new LingoSpriteAnimator(this, _environment);
                AddActor(animator);
            }

            animator.AddKeyFrames(keyframes);
        }

        public void UpdateKeyframe(int frame, float x, float y, float rotation, float skew)
        {
            var animator = _spriteActors.OfType<LingoSpriteAnimator>().FirstOrDefault();
            if (animator == null)
            {
                animator = new LingoSpriteAnimator(this, _environment);
                AddActor(animator);
            }
            animator.UpdateKeyFrame(frame, x, y, rotation, skew);
            animator.RecalculateCache();
        }

        public void SetSpriteTweenOptions(bool positionEnabled, bool rotationEnabled, bool skewEnabled,
            bool foregroundColorEnabled, bool backgroundColorEnabled, bool blendEnabled,
            float curvature, bool continuousAtEnds, bool speedSmooth, float easeIn, float easeOut)
        {
            var animator = _spriteActors.OfType<LingoSpriteAnimator>().FirstOrDefault();
            if (animator == null)
            {
                animator = new LingoSpriteAnimator(this, _environment);
                AddActor(animator);
            }
            animator.SetTweenOptions(positionEnabled, rotationEnabled, skewEnabled,
                foregroundColorEnabled, backgroundColorEnabled, blendEnabled,
                curvature, continuousAtEnds, speedSmooth, easeIn, easeOut);
        }

        /*
        When the movie first starts, events occur in the following order:
1 prepareMovie
2 prepareFrame Immediately after the prepareFrame event, Director plays sounds, draws
sprites, and performs any transitions or palette effects. This event occurs before the enterFrame
event. A prepareFrame handler is a good location for script that you want to run before the
frame draws.
3 beginSprite This event occurs when the playhead enters a sprite span.
4 startMovie This event occurs in the first frame that plays.
34 Chapter 2: Director Scripting Essentials
When the movie encounters a frame, events occur in the following order:
1 beginSprite This event occurs only if new sprites begin in the frame.
2 stepFrame
3 prepareFrame
4 enterFrame After enterFrame and before exitFrame, Director handles any time delays
required by the tempo setting, idle events, and keyboard and mouse events.
5 exitFrame
6 endSprite This event occurs only if the playhead exits a sprite in the frame.
When a movie stops, events occur in the following order:
1 endSprite T
        2 stopMovie

        */

        internal virtual void DoBeginSprite()
        {
            // Subscribe all actors
            foreach (var actor in _spriteActors)
            {
                _eventMediator.Subscribe(actor);
                if (actor is IHasBeginSpriteEvent begin) begin.BeginSprite();
            }
            // Subscribe all behaviors
            _behaviors.ForEach(b =>
            {
                _eventMediator.Subscribe(b);
                if (b is IHasBeginSpriteEvent beginSpriteEvent) beginSpriteEvent.BeginSprite();

            });
            BeginSprite();
        }
        protected virtual void BeginSprite() { }
        internal virtual LingoMember? DoPreStepFrame()
        {
            if (_Member != null && _Member.HasChanged)
            {
                _frameworkSprite.ApplyMemberChanges();
                return _Member;
            }
            return null;
        }
        internal virtual void DoEndSprite()
        {
            _behaviors.ForEach(b =>
            {
                // Unsubscribe all behaviors
                _eventMediator.Unsubscribe(b);
                if (b is IHasEndSpriteEvent endSpriteEvent) endSpriteEvent.EndSprite();
            });
            foreach (var actor in _spriteActors)
            {
                if (actor is IHasEndSpriteEvent end) end.EndSprite();
                _eventMediator.Unsubscribe(actor);
            }
            EndSprite();
        }
        protected virtual void EndSprite() { }



        internal void SetFrameworkSprite(ILingoFrameworkSprite fw) => _frameworkSprite = fw;

        public bool PointInSprite(LingoPoint point)
        {
            return Rect.Contains(point);
        }

        public ILingoSprite SetMember(string memberName, int? castLibNum = null)
        {
            var member = _environment.GetMember<LingoMember>(memberName, castLibNum);
            _Member = member ?? throw new Exception(Name + ":Member not found with name " + memberName);
            SetMember(member);
            return this;
        }


        public ILingoSprite SetMember(int memberNumber, int? castLibNum = null)
        {
            var member = _environment.GetMember<LingoMember>(memberNumber, castLibNum);
            _Member = member ?? throw new Exception(Name + ":Member not found with number: " + memberNumber);
            SetMember(member);
            return this;
        }
        public ILingoSprite SetMember(ILingoMember? member)
        {
            _Member = member as LingoMember;
            if (_Member != null)
            {
                RegPoint = _Member.RegPoint;
            }


            MemberHasChanged();
            return this;
        }

        private void MemberHasChanged()
        {
            var existingPlayer = _spriteActors.OfType<LingoFilmLoopPlayer>().FirstOrDefault();
            if (_Member is LingoMemberFilmLoop)
            {
                if (existingPlayer == null)
                {
                    existingPlayer = new LingoFilmLoopPlayer(this, _environment);
                    AddActor(existingPlayer);
                }
                if (IsActive)
                    existingPlayer.BeginSprite();
            }
            else if (existingPlayer != null)
            {
                if (IsActive)
                    existingPlayer.EndSprite();
                RemoveActor(existingPlayer);
            }

            _frameworkSprite.MemberChanged();
        }

        #region ZIndex/locZ
        public void SendToBack()
        {
            LocZ = 1;
        }

        public void BringToFront()
        {
            var maxZ = ((LingoMovie)_environment.Movie).GetMaxLocZ();
            LocZ = maxZ + 1;
        }

        public void MoveBackward()
        {
            LocZ--;
        }

        public void MoveForward()
        {
            LocZ++;
        }
        #endregion
        public LingoSprite Duplicate()
        {
            throw new NotImplementedException();
            // Create shallow copy, link same member, etc.
            //return new LingoSprite(_env, Score, Name + "_copy", SpriteNum)
            //{
            //    Member = Member,
            //    LocH = LocH,
            //    LocV = LocV,
            //    Blend = Blend,
            //    Visible = Visible,
            //    Color = Color,
            //    //Rect = Rect
            //    // etc.
            //};
        }


        #region Math methods

        public bool Intersects(ILingoSprite other)
        {
            return Rect.Intersects(other.Rect);
        }

        public bool Within(ILingoSprite other)
        {
            var center = Rect.Center;
            return other.Rect.Contains(center);
        }

        public (LingoPoint topLeft, LingoPoint topRight, LingoPoint bottomRight, LingoPoint bottomLeft) Quad()
        {
            var rect = Rect;
            return (
                rect.TopLeft,
                new LingoPoint(rect.Right, rect.Top),
                rect.BottomRight,
                new LingoPoint(rect.Left, rect.Bottom)
            );
        }

        #endregion


        #region Mouse

        void ILingoMouseEventHandler.RaiseMouseMove(LingoMouse mouse)
        {
            // Only respond if the sprite is active and the mouse is within the bounding box
            if (IsActive && IsMouseInsideBoundingBox(mouse))
            {
                _eventMediator.RaiseMouseMove(mouse);
                if (!isMouseInside)
                {
                    MouseEnter(mouse); // Mouse has entered the sprite
                    _eventMediator.RaiseMouseEnter(mouse);
                    isMouseInside = true;
                }
            }
            else
            {
                if (isMouseInside)
                {
                    MouseExit(mouse); // Mouse has exited the sprite
                    _eventMediator.RaiseMouseExit(mouse);
                    isMouseInside = false;
                }
            }
            if (IsActive && isDraggable && isDragging)
                DoMouseDrag(mouse);
        }
        public virtual void MouseMove(LingoMouse mouse)
        {

        }
        /// <summary>
        /// Triggered when the mouse enters the sprite's bounds
        /// </summary>
        public virtual void MouseEnter(LingoMouse mouse)
        {
            if (_cursor != 0)
            {
                _previousCursor = mouse.Cursor.Cursor;
                mouse.Cursor.Cursor = _cursor;
            }
        }
        /// <summary>
        /// Triggered when the mouse exits the sprite's bounds
        /// </summary>
        public virtual void MouseExit(LingoMouse mouse)
        {
            if (_previousCursor.HasValue)
            {
                mouse.Cursor.Cursor = _previousCursor.Value;
                _previousCursor = null;
            }
        }
        void ILingoMouseEventHandler.RaiseMouseDown(LingoMouse mouse)
        {
            if (isDraggable && IsMouseInsideBoundingBox(mouse))
                isDragging = true;
            MouseDown(mouse);
            _eventMediator.RaiseMouseDown(mouse);
        }

        protected virtual void MouseDown(LingoMouse mouse) { }
        void ILingoMouseEventHandler.RaiseMouseUp(LingoMouse mouse)
        {
            if (isDragging && isDragging)
                isDragging = false;
            MouseUp(mouse);
            _eventMediator.RaiseMouseUp(mouse);
        }
        protected virtual void MouseUp(LingoMouse mouse) { }
        private void DoMouseDrag(LingoMouse mouse)
        {
            LocH = mouse.MouseH;
            LocV = mouse.MouseV;
            //_environment.Player.Stage.AddKeyFrame(this);
            MouseDrag(mouse);
        }
        protected virtual void MouseDrag(LingoMouse mouse) { }


        #endregion



        #region Blur/Focus
        public virtual void Focus()
        {
            if (_isFocus) return;
            _isFocus = true;
            _eventMediator.RaiseFocus();
        }
        public virtual void Blur()
        {
            if (!_isFocus) return;
            _isFocus = false;
            _eventMediator.RaiseBlur();
        }
        #endregion



        /// <summary>
        /// Check if the mouse position is inside the bounding box of the sprite
        /// </summary>
        public bool IsMouseInsideBoundingBox(LingoMouse mouse)
            => Rect.Contains((mouse.MouseH, mouse.MouseV));

        /// <summary>
        /// Check if the given point is inside the bounding box of the sprite
        /// </summary>
        public bool IsPointInsideBoundingBox(float x, float y)
            => Rect.Contains((x, y));

        internal void CallBehavior<T>(Action<T> actionOnSpriteBehaviour) where T : LingoSpriteBehavior
        {
            var behavior = _behaviors.FirstOrDefault(x => x is T) as T;
            if (behavior == null) return;
            actionOnSpriteBehaviour(behavior);
        }
        internal TResult? CallBehavior<T, TResult>(Func<T, TResult> actionOnSpriteBehaviour) where T : LingoSpriteBehavior
        {
            var behavior = _behaviors.FirstOrDefault(x => x is T) as T;
            if (behavior == null) return default;
            return actionOnSpriteBehaviour(behavior);
        }

        internal void CallActor<T>(Action<T> actionOnActor) where T : class
        {
            var actor = _spriteActors.OfType<T>().FirstOrDefault();
            if (actor == null) return;
            actionOnActor(actor);
        }

        internal TResult? CallActor<T, TResult>(Func<T, TResult> func) where T : class
        {
            var actor = _spriteActors.OfType<T>().FirstOrDefault();
            if (actor == null) return default;
            return func(actor);
        }

        private void AddActor(object actor)
        {
            _spriteActors.Add(actor);
            if (IsActive)
            {
                _eventMediator.Subscribe(actor);
                if (actor is IHasBeginSpriteEvent begin) begin.BeginSprite();
            }
        }

        private void RemoveActor(object actor)
        {
            if (IsActive)
            {
                if (actor is IHasEndSpriteEvent end) end.EndSprite();
                _eventMediator.Unsubscribe(actor);
            }
            _spriteActors.Remove(actor);
        }


        public void RemoveMe()
        {
            _frameworkSprite.RemoveMe();
            if (_onRemoveMe != null)
                _onRemoveMe(this);
        }

        public void SetOnRemoveMe(Action<LingoSprite> onRemoveMe) => _onRemoveMe = onRemoveMe;

        public string GetFullName() => $"{SpriteNum}.{Name}.{Member?.Name}";

        internal void ChangeSpriteNumIKnowWhatImDoingOnlyInternal(int spriteNum)
        {
            SpriteNum = spriteNum;
        }
    }
}
