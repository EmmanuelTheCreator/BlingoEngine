using LingoEngine.Animations;
using LingoEngine.Casts;
using LingoEngine.Members;
using LingoEngine.Primitives;
using LingoEngine.Sprites;

namespace LingoEngine.FilmLoops
{

    /// <summary>
    /// Declares a sprite from a filmloop inside a member, not instanciated as sprite yet.
    /// </summary>
    public class LingoFilmLoopMemberSprite
    {
        private int _ink;
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

        public LingoPoint RegPoint { get; set; }
        public LingoColor ForeColor { get; set; } = LingoColorList.Black;
        public LingoColor BackColor { get; set; } = LingoColorList.White;
        public ILingoMember? Member { get; private set; }

        public float Width { get; set; }
        public float Height { get; set; }


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
                    if (Member != null && (Width == 0 || Height == 0))
                    {
                        Width = Member.Width;
                        Height = Member.Height;
                    }
                }
            }
        }

        public LingoRect GetBoundingBox() => _animatorProperties.GetBoundingBox(RegPoint, Rect, Width, Height);
        public LingoRect GetBoundingBoxForFrame(int frame) => _animatorProperties.GetBoundingBoxForFrame(frame, RegPoint, Width, Height);


        private LingoPoint GetRegPointOffset()
        {
            if (Member != null)
            {
                var baseOffset = Member.CenterOffsetFromRegPoint();
                if (Member.Width != 0 && Member.Height != 0)
                {
                    float scaleX = Width / Member.Width;
                    float scaleY = Height / Member.Height;
                    return new LingoPoint(baseOffset.X * scaleX, baseOffset.Y * scaleY);
                }
                return baseOffset;
            }
            return new LingoPoint();
        }
        public void AddKeyframes(params LingoKeyFrameSetting[] keyframes) => AnimatorProperties.AddKeyframes(keyframes);

    }
}
