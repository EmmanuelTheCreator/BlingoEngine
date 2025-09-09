using LingoEngine.Events;
using LingoEngine.Inputs;
using LingoEngine.Primitives;
using LingoEngine.Medias;
using LingoEngine.Animations;
using LingoEngine.Members;
using LingoEngine.Casts;
using LingoEngine.Movies;
using LingoEngine.FilmLoops;
using LingoEngine.Sprites.Events;
using LingoEngine.Bitmaps;
using LingoEngine.FrameworkCommunication;
using System.Diagnostics;
using LingoEngine.Inputs.Events;
using AbstUI.Primitives;

namespace LingoEngine.Sprites
{
    [DebuggerDisplay("LSprite2D:{SpriteNum}) {Name}:Member={Member?.Name}:{BeginFrame}->{EndFrame}:Pos={LocH}x{LocV}:Size={Width}x{Height}")]
    public class LingoSprite2D : LingoSprite, ILingoMouseEventHandler, ILingoSprite, ILingoSpriteWithMember, ILingoSprite2DLight
    {
        public const int SpriteNumOffset = 6;
        private readonly List<LingoSpriteBehavior> _behaviors = new();
        private readonly LingoMovie _movie;
        private readonly ILingoFrameworkFactory _frameworkFactory;
        private readonly ILingoSpritesPlayer _spritesHolder;
        private readonly ILingoMovieEnvironment _environment;

        public IReadOnlyList<LingoSpriteBehavior> Behaviors => _behaviors;


        private bool _isMouseInside = false;
        private bool _isDragging = false;
        private bool _isDraggable = false;  // A flag to control dragging behavior
        private LingoMember? _member;
        private Action<LingoSprite2D>? _onRemoveMe;
        private bool _isFocus = false;
        private IAbstUITextureUserSubscription? _textureSubscription;
        private bool _flipH;
        private bool _flipV;
        private int _cursor;
        private int? _previousCursor;
        private int _constraint;
        private bool _directToStage;
        private float _blend = 100f;


        #region Properties
        public override int SpriteNumWithChannel => SpriteNum + SpriteNumOffset;
        internal LingoSpriteChannel? SpriteChannel { get; set; }

        /// <inheritdoc/>
        public T Framework<T>() where T : class, ILingoFrameworkSprite => (T)_frameworkSprite;



        public override string Name { get => _frameworkSprite.Name; set => _frameworkSprite.Name = value; }
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
        public APoint Loc { get => (_frameworkSprite.X, _frameworkSprite.Y); set => _frameworkSprite.SetPosition(value); }

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

        public APoint RegPoint { get; set; }
        public AColor ForeColor { get; set; } = AColors.Black;
        public AColor BackColor { get; set; } = AColors.White;
        public List<string> ScriptInstanceList { get; private set; } = new();



        public ILingoMember? Member { get => _member; set => SetMember(value); }
        public LingoCast? Cast { get; private set; }


        public bool Editable { get; set; }

        public bool IsDraggable
        {
            get => _isDraggable;
            set => _isDraggable = value;
        }

        public AColor Color { get; set; }

        public ARect Rect
        {
            get
            {
                var offset = GetRegPointOffset();
                float left = LocH - offset.X - Width / 2f;
                float top = LocV - offset.Y - Height / 2f;
                return new ARect(left, top, left + Width, top + Height);
            }
        }

        public int Size => Media.Length;

        public byte[] Media { get; set; } = new byte[] { };
        public byte[] Thumbnail { get; set; } = new byte[] { };
        public string ModifiedBy { get; set; } = "";

        /// <inheritdoc/>
        public void Play()
        {
            if (_frameworkSprite is ILingoFrameworkSpriteVideo video)
                video.Play();
            _eventMediator.RaiseStartVideo();
        }

        /// <inheritdoc/>
        public void Stop()
        {
            if (_frameworkSprite is ILingoFrameworkSpriteVideo video)
                video.Stop();
            _eventMediator.RaiseStopVideo();
            _eventMediator.RaiseEndVideo();
        }

        /// <inheritdoc/>
        public void Pause()
        {
            if (_frameworkSprite is ILingoFrameworkSpriteVideo video)
                video.Pause();
            _eventMediator.RaisePauseVideo();
        }

        /// <inheritdoc/>
        public void Seek(int milliseconds)
        {
            if (_frameworkSprite is ILingoFrameworkSpriteVideo video)
                video.Seek(milliseconds);
        }

        /// <inheritdoc/>
        public int Duration => (_frameworkSprite as ILingoFrameworkSpriteVideo)?.Duration ?? 0;

        /// <inheritdoc/>
        public int CurrentTime
        {
            get => (_frameworkSprite as ILingoFrameworkSpriteVideo)?.CurrentTime ?? 0;
            set
            {
                if (_frameworkSprite is ILingoFrameworkSpriteVideo video)
                    video.CurrentTime = value;
            }
        }

        /// <inheritdoc/>
        public LingoMediaStatus MediaStatus => (_frameworkSprite as ILingoFrameworkSpriteVideo)?.MediaStatus ?? LingoMediaStatus.Closed;

        public float Width
        {
            get => _frameworkSprite.Width;
            set => _frameworkSprite.DesiredWidth = value;
        }
        public float Height
        {
            get => _frameworkSprite.Height;
            set => _frameworkSprite.DesiredHeight = value;
        }

        public ILingoMember? GetMember() => Member;


        #endregion

        private void ApplyBlend()
        {
            _frameworkSprite.Blend = _directToStage ? 1f : _blend;
        }

        private APoint GetRegPointOffset()
        {
            if (_member is { } member)
            {
                var baseOffset = member.CenterOffsetFromRegPoint();
                if (member.Width != 0 && member.Height != 0)
                {
                    float scaleX = Width / member.Width;
                    float scaleY = Height / member.Height;
                    return new APoint(baseOffset.X * scaleX, baseOffset.Y * scaleY);
                }
                return baseOffset;
            }
            return new APoint();
        }



        // Not used in c#
        // public int ScriptText { get; set; }

#pragma warning disable CS8618
        public LingoSprite2D(ILingoMovieEnvironment environment, ILingoSpritesPlayer spritesHolder) : base(environment.Events)
#pragma warning restore CS8618
        {
            _movie = (LingoMovie)environment.Movie;
            _frameworkFactory = environment.Factory;
            _spritesHolder = spritesHolder;
            _environment = environment;
        }
        public void Init(ILingoFrameworkSprite frameworkSprite)
        {
            _frameworkSprite = frameworkSprite;
            _frameworkSprite.Ink = _ink;
            ApplyBlend();
        }

        #region Behaviors

        public ILingoSprite AddBehavior<T>(Action<T>? configure = null) where T : LingoSpriteBehavior
        {
            SetBehavior<T>(configure);
            return this;
        }
        public T SetBehavior<T>(Action<T>? configure = null) where T : LingoSpriteBehavior
        {
            var behavior = _frameworkFactory.CreateBehavior<T>(_movie);
            behavior.SetMe(this);
            _behaviors.Add(behavior);
            if (configure != null)
                configure(behavior);
            return behavior;
        }
        private void CallBehaviorForEvents<T>(Action<T> action)
        {
            foreach (var item in _behaviors.OfType<T>())
                action(item);
        }
        #endregion


        #region Animation / keyframes

        /// <summary>
        /// Adds animation keyframes for this sprite. When invoked for the first time
        /// it lazily creates a <see cref="LingoSpriteAnimator"/> actor and stores it
        /// in the internal sprite actors list.
        /// </summary>
        /// <param name="keyframes">Tuple list containing frame number, X, Y, rotation and skew values.</param>
        public void AddKeyframes(params LingoKeyFrameSetting[] keyframes)
        {
            if (keyframes == null || keyframes.Length == 0)
                return;
            GetAnimator().AddKeyFrames(keyframes);
        }

        public void UpdateKeyframe(LingoKeyFrameSetting setting)
        {
            var animator = GetAnimator();
            animator.UpdateKeyFrame(setting);
            animator.RecalculateCache();
        }
        public IReadOnlyCollection<LingoKeyFrameSetting>? GetKeyframes()
        {
            var animator = GetActorsOfType<LingoSpriteAnimator>().FirstOrDefault();
            if (animator == null)
                return null;
            return animator.GetKeyframes();
        }

        public void MoveKeyFrame(int from, int to)
        {
            var animator = GetActorsOfType<LingoSpriteAnimator>().FirstOrDefault();
            if (animator == null)
                return;
            animator.MoveKeyFrame(from, to);
        }
        public bool DeleteKeyFrame(int frame)
        {
            var animator = GetActorsOfType<LingoSpriteAnimator>().FirstOrDefault();
            if (animator == null)
                return false;
            return animator.DeleteKeyFrame(frame);
        }

        public void SetSpriteTweenOptions(bool positionEnabled, bool sizeEnabled, bool rotationEnabled, bool skewEnabled,
            bool foregroundColorEnabled, bool backgroundColorEnabled, bool blendEnabled,
            float curvature, bool continuousAtEnds, bool speedSmooth, float easeIn, float easeOut)
        {
            GetAnimator().SetTweenOptions(positionEnabled, sizeEnabled, rotationEnabled, skewEnabled,
                foregroundColorEnabled, backgroundColorEnabled, blendEnabled,
                curvature, continuousAtEnds, speedSmooth, easeIn, easeOut);
        }
        private LingoSpriteAnimator GetAnimator()
        {
            var animator = GetActorsOfType<LingoSpriteAnimator>().FirstOrDefault();
            if (animator == null)
            {
                animator = new LingoSpriteAnimator(this, _movie, _eventMediator);
                AddActor(animator);
            }

            return animator;
        }
        public void AddAnimator(LingoSpriteAnimatorProperties animatorProps, ILingoEventMediator eventMediator)
        {
            var animator = GetActorsOfType<LingoSpriteAnimator>().FirstOrDefault();
            if (animator != null) throw new Exception("Animator already set");
            animator = new LingoSpriteAnimator(this, _movie, eventMediator, animatorProps);
            AddActor(animator);
        }
        #endregion
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

        internal override void DoBeginSprite()
        {
            if (_textureSubscription == null && _member != null)
            {
                _member.UsedBy(this);
                MemberHasChanged();
            }
            // Subscribe all behaviors
            _behaviors.ForEach(b =>
            {
                _eventMediator.Subscribe(b, SpriteNum + 6, true); //  we ignore mouse because it has to be within the boundingbox
                if (_movie.IsPlaying)
                    if (b is IHasBeginSpriteEvent beginSpriteEvent) beginSpriteEvent.BeginSprite();

            });
            base.DoBeginSprite();
        }
        internal virtual LingoMember? DoPreStepFrame()
        {
            if (_member != null && _member.HasChanged)
            {
#if DEBUG
                if (_member.NumberInCast == 44)
                {

                }
#endif
                _frameworkSprite.ApplyMemberChangesOnStepFrame();
                return _member;
            }
            return null;
        }
        internal override void DoEndSprite()
        {
            _behaviors.ForEach(b =>
            {
                // Unsubscribe all behaviors
                _eventMediator.Unsubscribe(b, true); //  we ignore mouse because it has to be within the boundingbox
                if (_movie.IsPlaying)
                    if (b is IHasEndSpriteEvent endSpriteEvent) endSpriteEvent.EndSprite();
            });
            FrameworkObj.Hide();
            _textureSubscription?.Release();
            _textureSubscription = null;
            // Release the old member link with this sprite
            _member?.ReleaseFromRefUser(this);
            base.DoEndSprite();
        }




        internal void SetFrameworkSprite(ILingoFrameworkSprite fw) => _frameworkSprite = fw;

        public bool PointInSprite(APoint point)
        {
            return Rect.Contains(point);
        }


        #region Member / MemberChange

        public ILingoSprite SetMember(string memberName, int? castLibNum = null)
        {
            var member = _environment.GetMember<LingoMember>(memberName, castLibNum);
            var newMember = member ?? throw new Exception(Name + ":Member not found with name " + memberName);
            SetMember(newMember);
            return this;
        }


        public ILingoSprite SetMember(int memberNumber, int? castLibNum = null)
        {
            var member = _environment.GetMember<LingoMember>(memberNumber, castLibNum);

            var newMember = member ?? throw new Exception(Name + ":Member not found with number: " + memberNumber);
            SetMember(newMember);
            return this;
        }
        public ILingoSprite SetMember(ILingoMember? member)
        {
            if (_member == member) return this;
            var _lastMember = _member;
            if (_member != null && (_member.Type == LingoMemberType.Script || _member.Type == LingoMemberType.Sound || _member.Type == LingoMemberType.Transition || _member.Type == LingoMemberType.Unknown || _member.Type == LingoMemberType.Palette || _member.Type == LingoMemberType.Movie || _member.Type == LingoMemberType.Font || _member.Type == LingoMemberType.Cursor))
                return this;
            // Release the old member link with this sprite
            if (member != _member)
            {
                _member?.ReleaseFromRefUser(this);
            }
            _member = member as LingoMember;
            if (_member != null)
            {
                _member.UsedBy(this);
                RegPoint = _member.RegPoint;
            }
            if (member is LingoFilmLoopMember filmLoop)
            {
                filmLoop.PrepareFilmloop();// to get the size
                Width = filmLoop.Width;
                Height = filmLoop.Height;
            }

            MemberHasChanged();
            return this;
        }

        private void MemberHasChanged()
        {
            var existingPlayer = GetActorsOfType<LingoFilmLoopPlayer>().FirstOrDefault();
            if (_member is LingoFilmLoopMember)
            {
                if (existingPlayer == null)
                {
                    existingPlayer = new LingoFilmLoopPlayer(this, _eventMediator, _environment.CastLibsContainer);
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
        #endregion


        public LingoFilmLoopPlayer? GetFilmLoopPlayer() => GetActorsOfType<LingoFilmLoopPlayer>().FirstOrDefault();


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
        public LingoSprite2D Duplicate()
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

        public (APoint topLeft, APoint topRight, APoint bottomRight, APoint bottomLeft) Quad()
        {
            var rect = Rect;
            return (
                rect.TopLeft,
                new APoint(rect.Right, rect.Top),
                rect.BottomRight,
                new APoint(rect.Left, rect.Bottom)
            );
        }

        #endregion


        #region Mouse

        public void RaiseMouseMove(LingoMouseEvent mouse)
        {
            if (!_movie.IsPlaying) return;
            //if (IsActive && Member?.Name == "B_Play")
            //{
            //    var inside = IsMouseInsideBoundingBox(mouse.Mouse);
            //    Console.WriteLine($"Mouse:{mouse.Mouse.MouseH}x{mouse.Mouse.MouseV} \t{Member?.Name}\t{inside}\tpos={LocH}x{LocV}\tRect={Rect};Size: {Width}x{Height}");
            //}
            // Only respond if the sprite is active and the mouse is within the bounding box
            if (IsActive && IsMouseInsideBoundingBox(mouse.Mouse))
            {

                CallBehaviorForEvents<IHasMouseMoveEvent>(b => b.MouseMove(mouse));
                _eventMediator.RaiseMouseMove(mouse);

                if (!_isMouseInside)
                {
                    MouseEnter(mouse); // Mouse has entered the sprite
                    CallBehaviorForEvents<IHasMouseEnterEvent>(b => b.MouseEnter(mouse));
                    _eventMediator.RaiseMouseEnter(mouse);
                    _isMouseInside = true;
                }
            }
            else
            {
                if (_isMouseInside)
                {
                    MouseExit(mouse); // Mouse has exited the sprite
                    CallBehaviorForEvents<IHasMouseExitEvent>(b => b.MouseExit(mouse));
                    _eventMediator.RaiseMouseExit(mouse);
                    _isMouseInside = false;
                }
            }
            if (IsActive && _isDraggable && _isDragging)
                DoMouseDrag(mouse);
        }
        public virtual void MouseMove(LingoMouseEvent mouse)
        {

        }
        /// <summary>
        /// Triggered when the mouse enters the sprite's bounds
        /// </summary>
        public virtual void MouseEnter(LingoMouseEvent mouse)
        {
            if (_cursor != 0)
            {
                _previousCursor = mouse.Mouse.Cursor.Cursor;
                mouse.Mouse.Cursor.Cursor = _cursor;
            }
        }
        /// <summary>
        /// Triggered when the mouse exits the sprite's bounds
        /// </summary>
        public virtual void MouseExit(LingoMouseEvent mouse)
        {
            if (_previousCursor.HasValue)
            {
                mouse.Mouse.Cursor.Cursor = _previousCursor.Value;
                _previousCursor = null;
            }
        }
        public void RaiseMouseDown(LingoMouseEvent mouse)
        {
            if (_isDraggable && IsMouseInsideBoundingBox(mouse.Mouse))
                _isDragging = true;
            if (_isMouseInside)
            {
                MouseDown(mouse);
                CallBehaviorForEvents<IHasMouseDownEvent>(b => b.MouseDown(mouse));
                _eventMediator.RaiseMouseDown(mouse);
            }
        }

        protected virtual void MouseDown(LingoMouseEvent mouse) { }
        public void RaiseMouseUp(LingoMouseEvent mouse)
        {
            if (_isDragging && _isDragging)
                _isDragging = false;
            if (_isMouseInside)
            {
                MouseUp(mouse);
                CallBehaviorForEvents<IHasMouseUpEvent>(b => b.MouseUp(mouse));
                _eventMediator.RaiseMouseUp(mouse);
            }
        }
        protected virtual void MouseUp(LingoMouseEvent mouse) { }
        public void RaiseMouseWheel(LingoMouseEvent mouse)
        {
            if (_isMouseInside)
            {
                CallBehaviorForEvents<IHasMouseWheelEvent>(b => b.MouseWheel(mouse));
                _eventMediator.RaiseMouseWheel(mouse);
            }
        }

        private void DoMouseDrag(LingoMouseEvent mouse)
        {
            LocH = mouse.MouseH;
            LocV = mouse.MouseV;
            //_environment.Player.Stage.AddKeyFrame(this);
            MouseDrag(mouse);
        }
        protected virtual void MouseDrag(LingoMouseEvent mouse) { }


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
        /// Determines whether the mouse is inside the sprite, considering
        /// transparent pixels for <see cref="LingoInkType.BackgroundTransparent"/>.
        /// </summary>
        public bool IsMouseInsideBoundingBox(LingoMouse mouse)
        {
            var rect = Rect; // LingoRect.New(LocH,LocV,Width,Height);
            if (!rect.Contains(mouse.MouseLoc))
                return false;

            if (InkType != LingoInkType.BackgroundTransparent || Member == null)
                return true;

            //var rect = Rect;
            if (Member.Width == 0 || Member.Height == 0 || Width == 0 || Height == 0)
                return true;

            var relX = (mouse.MouseH - rect.Left) / Width;
            var relY = (mouse.MouseV - rect.Top) / Height;

            int pixelX = (int)(relX * Member.Width);
            int pixelY = (int)(relY * Member.Height);

            if (FlipH)
                pixelX = Member.Width - 1 - pixelX;
            if (FlipV)
                pixelY = Member.Height - 1 - pixelY;

            return !Member.IsPixelTransparent(pixelX, pixelY);
        }

        /// <summary>
        /// Check if the given point is inside the bounding box of the sprite
        /// </summary>
        public bool IsPointInsideBoundingBox(float x, float y)
            => Rect.Contains((x, y));

        internal void CallBehavior<T>(Action<T> actionOnSpriteBehaviour) where T : ILingoSpriteBehavior
        {
            var behavior = _behaviors.OfType<T>().FirstOrDefault();
            if (behavior == null) return;
            actionOnSpriteBehaviour(behavior);
        }
        internal TResult? CallBehavior<T, TResult>(Func<T, TResult> actionOnSpriteBehaviour) where T : ILingoSpriteBehavior
        {
            var behavior = _behaviors.OfType<T>().FirstOrDefault();
            if (behavior == null) return default;
            return actionOnSpriteBehaviour(behavior);
        }





        public override void OnRemoveMe()
        {
            _member?.ReleaseFromRefUser(this);
            _frameworkSprite.RemoveMe();
            if (_onRemoveMe != null)
                _onRemoveMe(this);
        }

        public void MemberHasBeenRemoved()
        {
            _member = null;
        }

        public void SetOnRemoveMe(Action<LingoSprite2D> onRemoveMe) => _onRemoveMe = onRemoveMe;

        public override string GetFullName() => $"{SpriteNum}.{Name}.{Member?.Name}";

        internal void ChangeSpriteNumIKnowWhatImDoingOnlyInternal(int spriteNum)
        {
            SpriteNum = spriteNum;
        }
        #region Clone and Get/Load State

        public override Action<LingoSprite> GetCloneAction()
        {
            var baseAction = base.GetCloneAction();
            Action<LingoSprite> action = s => { };
            var member = Member;
            var x = LocH;
            var y = LocV;
            var z = LocZ;
            var width = Width;
            var height = Height;
            var skew = Skew;
            var ink = Ink;
            var blend = Blend;
            var hilite = Hilite;
            var rotation = Rotation;
            var flipH = FlipH;
            var flipV = FlipV;
            var cursor = Cursor;
            var foreColor = ForeColor;
            var backColor = BackColor;
            var editable = Editable;
            var isDraggable = IsDraggable;
            action = s =>
            {
                baseAction(s);
                var sprite2D = (LingoSprite2D)s;
                sprite2D.SetMember(member);
                sprite2D.LocH = x;
                sprite2D.LocV = y;
                sprite2D.LocZ = z;
                sprite2D.Width = width;
                sprite2D.Height = height;
                sprite2D.Skew = skew;
                sprite2D.Ink = ink;
                sprite2D.Blend = blend;
                sprite2D.Hilite = hilite;
                sprite2D.Rotation = rotation;
                sprite2D.FlipH = flipH;
                sprite2D.FlipV = flipV;
                sprite2D.Cursor = cursor;
                sprite2D.ForeColor = foreColor;
                sprite2D.BackColor = backColor;
                sprite2D.Editable = editable;
                sprite2D.IsDraggable = isDraggable;
            };

            return action;
        }

        protected override LingoSpriteState CreateState() => new LingoSprite2DState();

        protected override void OnLoadState(LingoSpriteState state)
        {
            if (state is not LingoSprite2DState s) return;
            if (s.Width > 0) Width = s.Width;
            if (s.Height > 0) Height = s.Height;
            //SetMember(s.Member); // Gives problems in SDL
            DisplayMember = s.DisplayMember;
            SpritePropertiesOffset = s.SpritePropertiesOffset;
            Ink = s.Ink;
            Hilite = s.Hilite;
            Blend = s.Blend;
            LocH = s.LocH;
            LocV = s.LocV;
            LocZ = s.LocZ;

            Rotation = s.Rotation;
            Skew = s.Skew;
            FlipH = s.FlipH;
            FlipV = s.FlipV;
            Cursor = s.Cursor;
            Constraint = s.Constraint;
            DirectToStage = s.DirectToStage;
            RegPoint = s.RegPoint;
            ForeColor = s.ForeColor;
            BackColor = s.BackColor;
            Editable = s.Editable;
            IsDraggable = s.IsDraggable;
        }

        protected override void OnGetState(LingoSpriteState state)
        {
            if (state is not LingoSprite2DState s) return;
            s.Member = (LingoMember?)Member;
            s.DisplayMember = DisplayMember;
            s.SpritePropertiesOffset = SpritePropertiesOffset;
            s.Ink = Ink;
            s.Hilite = Hilite;
            s.Blend = Blend;
            s.LocH = LocH;
            s.LocV = LocV;
            s.LocZ = LocZ;
            s.Width = Width;
            s.Height = Height;
            s.Rotation = Rotation;
            s.Skew = Skew;
            s.FlipH = FlipH;
            s.FlipV = FlipV;
            s.Cursor = Cursor;
            s.Constraint = Constraint;
            s.DirectToStage = DirectToStage;
            s.RegPoint = RegPoint;
            s.ForeColor = ForeColor;
            s.BackColor = BackColor;
            s.Editable = Editable;
            s.IsDraggable = IsDraggable;
        }

        #endregion
        public void UpdateTexture(IAbstTexture2D texture)
        {
            _frameworkSprite.SetTexture(texture);
        }

        public void FWTextureHasChanged(IAbstTexture2D? texture2D, bool useSubscription = true)
        {
            if (useSubscription)

            {
                _textureSubscription?.Release();
                if (texture2D != null)
                    _textureSubscription = texture2D.AddUser(this);
                else
                    _textureSubscription = null;
            }
        }
    }
}
