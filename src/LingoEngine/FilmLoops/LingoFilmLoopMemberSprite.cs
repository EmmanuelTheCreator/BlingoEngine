using AbstUI.Primitives;
using LingoEngine.Animations;
using LingoEngine.Casts;
using LingoEngine.Members;
using LingoEngine.Primitives;
using LingoEngine.Sprites;
using System.Diagnostics;

namespace LingoEngine.FilmLoops
{

    /// <summary>
    /// Declares a sprite from a filmloop inside a member, not instanciated as sprite yet.
    /// </summary>
    [DebuggerDisplay("LFilmLoopMemberSprite:{MemberNumberInCast}) {Name}:{BeginFrame}->{EndFrame}:Pos={LocH}x{LocV}:Size={Width}x{Height}")]
    public class LingoFilmLoopMemberSprite
    {
        private int _ink;
        private float width;
        private readonly LingoSpriteAnimatorProperties _animatorProperties;
        public LingoSpriteAnimatorProperties AnimatorProperties => _animatorProperties;

        public string Name { get; set; } = "";
        public int MemberNumberInCast { get; set; }
        public int CastNum { get; set; }
        public int DisplayMember { get; set; }
        public int SpriteNum { get; set; }

        public int Channel { get; set; }
        public int BeginFrame { get; set; }
        public int EndFrame { get; set; }

        public LingoInkType InkType { get => (LingoInkType)_ink; set => Ink = (int)value; }
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
        public ILingoMember? Member { get; private set; }

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

        private LingoFilmLoopMemberSprite()
        {
            // We need an animator to precalculate the path and bounding boxes.
            _animatorProperties = new LingoSpriteAnimatorProperties();
        }

        public LingoFilmLoopMemberSprite(ILingoMember? member = null) : this()
        {
            if (member != null)
                SetMember(member);
        }

        public LingoFilmLoopMemberSprite(LingoSprite2D sp, int channel, int begin, int end)
            : this(sp)
        {
            BeginFrame = begin;
            EndFrame = end;
            Channel = channel;
        }

        public LingoFilmLoopMemberSprite(LingoSprite2D sp)
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
            InkType = sp.InkType;
            DisplayMember = sp.DisplayMember;
            SpriteNum = sp.SpriteNum;
            Hilite = sp.Hilite;
            if (sp.Member != null)
                SetMember(sp.Member);
        }

        public void SetMember(ILingoMember? member)
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
        public void LinkMember(ILingoCastLibsContainer castLibs)
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

        private void SetMemberData(ILingoMember member)
        {
            if (member is LingoFilmLoopMember filmLoop)
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
        public void AddKeyframes(params LingoKeyFrameSetting[] keyframes) => AnimatorProperties.AddKeyframes(keyframes);

    }
}
