using AbstUI.Primitives;
using BlingoEngine.Animations;
using BlingoEngine.Casts;
using BlingoEngine.Members;
using BlingoEngine.Primitives;
using BlingoEngine.Sprites;
using System.Diagnostics;

namespace BlingoEngine.FilmLoops
{

    /// <summary>
    /// Declares a sprite from a filmloop inside a member, not instanciated as sprite yet.
    /// </summary>
    [DebuggerDisplay("LFilmLoopMemberSprite:{MemberNumberInCast}) {Name}:{BeginFrame}->{EndFrame}:Pos={LocH}x{LocV}:Size={Width}x{Height}")]
    public class BlingoFilmLoopMemberSprite
    {
        private int _ink;
        private float width;
        private readonly BlingoSpriteAnimatorProperties _animatorProperties;
        public BlingoSpriteAnimatorProperties AnimatorProperties => _animatorProperties;

        public string Name { get; set; } = "";
        public int MemberNumberInCast { get; set; }
        public int CastNum { get; set; }
        public int DisplayMember { get; set; }
        public int SpriteNum { get; set; }

        public int Channel { get; set; }
        public int BeginFrame { get; set; }
        public int EndFrame { get; set; }

        public BlingoInkType InkType { get => (BlingoInkType)_ink; set => Ink = (int)value; }
        public int Ink
        {
            get => _ink;
            set
            {
                _ink = value;
            }
        }

        public bool Hilite { get; set; }
        public float Blend { get; set; } = 100f;
        public float LocH { get; set; }
        public float LocV { get; set; }
        public int LocZ { get; set; }

        public float Rotation { get; set; }
        public float Skew { get; set; }
        public bool FlipH { get; set; }
        public bool FlipV { get; set; }

        public APoint RegPoint { get; set; }
        public AColor ForeColor { get; set; } = AColors.Black;
        public AColor BackColor { get; set; } = AColors.White;
        public IBlingoMember? Member { get; private set; }

        public float Width { 
            get => width; 
            set => width = value; 
        }
        public float Height { get; set; }


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

        public BlingoFilmLoopMemberSprite()
        {
            // We need an animator to precalculate the path and bounding boxes.
            _animatorProperties = new BlingoSpriteAnimatorProperties();
        }

        public BlingoFilmLoopMemberSprite(IBlingoMember member, int channel = 0, int begin = 0, int end = 0, int locH = 0, int locV = 0) : this()
        {
            if (member != null)
                SetMember(member);
            BeginFrame = begin;
            EndFrame = end;
            Channel = channel;
            LocH = locH;
            LocV = locV;
            if (Width == 0)
                Width = 50;
            if (Height == 0)
                Height = 50;
            InkType = BlingoInkType.Matte;
        }

        public BlingoFilmLoopMemberSprite(BlingoSprite2D sp, int channel, int begin, int end)
            : this(sp)
        {
            BeginFrame = begin;
            EndFrame = end;
            Channel = channel;
        }

        public BlingoFilmLoopMemberSprite(BlingoSprite2D sp)
            : this()
        {
            Name = sp.Name;
            LocH = sp.LocH;
            LocV = sp.LocV;
            LocZ = sp.LocZ;
            Rotation = sp.Rotation;
            Skew = sp.Skew;
            FlipH = sp.FlipH;
            FlipV = sp.FlipV;
            RegPoint = sp.RegPoint;
            ForeColor = sp.ForeColor;
            BackColor = sp.BackColor;
            Width = sp.Width;
            Height = sp.Height;
           // InkType = sp.InkType;
            DisplayMember = sp.DisplayMember;
            SpriteNum = sp.SpriteNum;
            Hilite = sp.Hilite;
            if (sp.Member != null)
                SetMember(sp.Member);
        }

        public void SetMember(IBlingoMember? member)
        {
            if (member == null)
            {
                return;
            }
            MemberNumberInCast = member.NumberInCast;
            CastNum = member.CastLibNum;
            Member = member;
            if (RegPoint == default)
                RegPoint = member.RegPoint;
            SetMemberData(member);
        }
        public void LinkMember(IBlingoCastLibsContainer castLibs)
        {
            if (Member != null) return;
            if (CastNum > 0 && MemberNumberInCast > 0)
            {
                var cast = castLibs.GetCast(CastNum);
                if (cast != null)
                {
                    Member = cast.Member[MemberNumberInCast];
                    if (Member != null)
                    {
                        if (RegPoint == default)
                            RegPoint = Member.RegPoint;
                        if (Width == 0 || Height == 0)
                            SetMemberData(Member);
                    }
                }
            }
        }

        private void SetMemberData(IBlingoMember member)
        {
            if (member is BlingoFilmLoopMember filmLoop)
            {
                filmLoop.UpdateSize();
            }
            if (member.Width == 0)
                member.Preload();
            Width = member.Width;
            Height = member.Height;
            _animatorProperties.RequestRecalculatedBoundingBox();
        }

        public ARect GetBoundingBox() => _animatorProperties.GetBoundingBox(RegPoint, Rect, Width, Height);
        public ARect GetBoundingBoxForFrame(int frame) => _animatorProperties.GetBoundingBoxForFrame(frame, RegPoint, Width, Height);


        private APoint GetRegPointOffset()
        {
            if (Member != null)
            {
                var baseOffset = Member.CenterOffsetFromRegPoint();
                if (Member.Width != 0 && Member.Height != 0)
                {
                    float scaleX = Width / Member.Width;
                    float scaleY = Height / Member.Height;
                    return new APoint(baseOffset.X * scaleX, baseOffset.Y * scaleY);
                }
                return baseOffset;
            }
            return new APoint();
        }
        /// <summary>
        /// Possible tuples : (frame,x,y) , (int frame, int x, int y, float width, float height)
        /// </summary>
        public void AddKeyframes(params BlingoKeyFrameSetting[] keyframes) => AnimatorProperties.AddKeyframes(keyframes);

    }
}

