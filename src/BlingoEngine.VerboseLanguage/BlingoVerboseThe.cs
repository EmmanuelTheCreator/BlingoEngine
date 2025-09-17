using System.Collections.Generic;
using AbstUI.Primitives;
using AbstUI.Texts;
using BlingoEngine.Bitmaps;
using BlingoEngine.Casts;
using BlingoEngine.Core;
using BlingoEngine.Medias;
using BlingoEngine.Members;
using BlingoEngine.Primitives;
using BlingoEngine.Shapes;
using BlingoEngine.Texts;

namespace BlingoEngine.VerboseLanguage
{
    /// <summary>
    /// the visibility of sprite 5
    /// </summary>
    public interface IBlingoVerboseThe
    {
        #region Sprites

        IBlingoVerboseTheOfSprite<int> BeginFrame { get; }
        IBlingoVerboseTheOfSprite<int> EndFrame { get; }
        IBlingoVerboseTheOfSprite<string> NameSprite { get; }
        IBlingoVerboseTheOfSprite<int> SpriteNum { get; }
        IBlingoVerboseTheOfSprite<bool> Visibility { get; }
        IBlingoVerboseTheOfSprite<bool> Puppet { get; }
        IBlingoVerboseTheOfSprite<bool> Lock { get; }
        IBlingoVerboseTheOfSprite<AColor> BackColor { get; }
        IBlingoVerboseTheOfSprite<float> Blend { get; }
        IBlingoVerboseTheOfSprite<BlingoCast?> CastSprite { get; }
        IBlingoVerboseTheOfSprite<AColor> Color { get; }
        IBlingoVerboseTheOfSprite<bool> Editable { get; }
        IBlingoVerboseTheOfSprite<AColor> ForeColor { get; }
        IBlingoVerboseTheOfSprite<bool> HiliteSprite { get; }
        IBlingoVerboseTheOfSprite<int> Ink { get; }
        IBlingoVerboseTheOfSprite<BlingoInkType> InkType { get; }
        IBlingoVerboseTheOfSprite<bool> Linked { get; }
        IBlingoVerboseTheOfSprite<bool> Loaded { get; }
        IBlingoVerboseTheOfSprite<byte[]> Media { get; }
        IBlingoVerboseTheOfSprite<bool> MediaReady { get; }
        IBlingoVerboseTheOfSprite<int> Duration { get; }
        IBlingoVerboseTheOfSprite<int> CurrentTime { get; }
        IBlingoVerboseTheOfSprite<BlingoMediaStatus> MediaStatus { get; }
        IBlingoVerboseTheOfSprite<float> WidthSprite { get; }
        IBlingoVerboseTheOfSprite<float> HeightSprite { get; }
        IBlingoVerboseTheOfSprite<IBlingoMember?> Member { get; }
        IBlingoVerboseTheOfSprite<string> ModifiedBy { get; }
        IBlingoVerboseTheOfSprite<ARect> Rect { get; }
        IBlingoVerboseTheOfSprite<APoint> RegPointSprite { get; }
        IBlingoVerboseTheOfSprite<APoint> Loc { get; }
        IBlingoVerboseTheOfSprite<float> LocH { get; }
        IBlingoVerboseTheOfSprite<float> LocV { get; }
        IBlingoVerboseTheOfSprite<int> LocZ { get; }
        IBlingoVerboseTheOfSprite<float> Rotation { get; }
        IBlingoVerboseTheOfSprite<float> Skew { get; }
        IBlingoVerboseTheOfSprite<bool> FlipH { get; }
        IBlingoVerboseTheOfSprite<bool> FlipV { get; }
        IBlingoVerboseTheOfSprite<float> Top { get; }
        IBlingoVerboseTheOfSprite<float> Bottom { get; }
        IBlingoVerboseTheOfSprite<float> Left { get; }
        IBlingoVerboseTheOfSprite<float> Right { get; }
        IBlingoVerboseTheOfSprite<int> Cursor { get; }
        IBlingoVerboseTheOfSprite<int> Constraint { get; }
        IBlingoVerboseTheOfSprite<bool> DirectToStage { get; }
        IBlingoVerboseTheOfSprite<List<string>> ScriptInstanceList { get; }
        IBlingoVerboseTheOfSprite<int> SizeSprite { get; }
        IBlingoVerboseTheOfSprite<byte[]> Thumbnail { get; }
        IBlingoVerboseTheOfSprite<int> MemberNum { get; }
        #endregion

        #region Members
        IBlingoVerboseTheOfMember<int> WidthMember { get; }
        IBlingoVerboseTheOfMember<int> HeightMember { get; }
        #endregion

        #region Text Members
        IBlingoVerboseTheOfMember<string> Text { get; }
        IBlingoVerboseTheOfMember<AbstTextAlignment> Alignment { get; }
        IBlingoVerboseTheOfMember<bool> EditableText { get; }
        IBlingoVerboseTheOfMember<bool> WordWrap { get; }
        IBlingoVerboseTheOfMember<int> ScrollTop { get; }
        IBlingoVerboseTheOfMember<string> Font { get; }
        IBlingoVerboseTheOfMember<int> FontSize { get; }
        IBlingoVerboseTheOfMember<BlingoTextStyle> FontStyle { get; }
        IBlingoVerboseTheOfMember<AColor> ColorText { get; }
        IBlingoVerboseTheOfMember<bool> Bold { get; }
        IBlingoVerboseTheOfMember<bool> Italic { get; }
        IBlingoVerboseTheOfMember<bool> Underline { get; }
        IBlingoVerboseTheOfMember<int> Margin { get; }
        IBlingoVerboseTheOfMember<BlingoLines> Line { get; }
        IBlingoVerboseTheOfMember<BlingoWords> Word { get; }
        IBlingoVerboseTheOfMember<BlingoChars> Char { get; }
        #endregion

        #region Bitmap Members
        IBlingoVerboseTheOfMember<byte[]?> ImageDataBitmap { get; }
        IBlingoVerboseTheOfMember<bool> IsLoadedBitmap { get; }
        IBlingoVerboseTheOfMember<string> FormatBitmap { get; }
        #endregion

        #region Shape Members
        IBlingoVerboseTheOfMember<BlingoList<APoint>> VertexList { get; }
        IBlingoVerboseTheOfMember<BlingoShapeType> ShapeType { get; }
        IBlingoVerboseTheOfMember<AColor> FillColor { get; }
        IBlingoVerboseTheOfMember<AColor> EndColor { get; }
        IBlingoVerboseTheOfMember<AColor> StrokeColor { get; }
        IBlingoVerboseTheOfMember<int> StrokeWidth { get; }
        IBlingoVerboseTheOfMember<bool> Closed { get; }
        IBlingoVerboseTheOfMember<bool> AntiAlias { get; }
        IBlingoVerboseTheOfMember<bool> Filled { get; }
        #endregion

    }



    /// <inheritdoc/>
    public partial record BlingoVerboseThe : BlingoVerboseBase, IBlingoVerboseThe
    {


        public BlingoVerboseThe(BlingoPlayer blingoPlayer)
            : base(blingoPlayer)
        {
        }


        #region Sprites

        public IBlingoVerboseTheOfSprite<int> BeginFrame => new BlingoTheTargetSprite<int>(_player, s => s.BeginFrame, (s, v) => s.BeginFrame = v);
        public IBlingoVerboseTheOfSprite<int> EndFrame => new BlingoTheTargetSprite<int>(_player, s => s.EndFrame, (s, v) => s.EndFrame = v);
        public IBlingoVerboseTheOfSprite<string> NameSprite => new BlingoTheTargetSprite<string>(_player, s => s.Name, (s, v) => s.Name = v);
        public IBlingoVerboseTheOfSprite<int> SpriteNum => new BlingoTheTargetSprite<int>(_player, s => s.SpriteNum, (s, v) => { });
        public IBlingoVerboseTheOfSprite<bool> Visibility => new BlingoTheTargetSprite<bool>(_player, s => s.Visibility, (s, v) => s.Visibility = v);
        public IBlingoVerboseTheOfSprite<bool> Puppet => new BlingoTheTargetSprite<bool>(_player, s => s.Puppet, (s, v) => s.Puppet = v);
        public IBlingoVerboseTheOfSprite<bool> Lock => new BlingoTheTargetSprite<bool>(_player, s => s.Lock, (s, v) => s.Lock = v);
        public IBlingoVerboseTheOfSprite<AColor> BackColor => new BlingoTheTargetSprite<AColor>(_player, s => s.BackColor, (s, v) => s.BackColor = v);
        public IBlingoVerboseTheOfSprite<float> Blend => new BlingoTheTargetSprite<float>(_player, s => s.Blend, (s, v) => s.Blend = v);
        public IBlingoVerboseTheOfSprite<BlingoCast?> CastSprite => new BlingoTheTargetSprite<BlingoCast?>(_player, s => s.Cast, (s, v) => { });
        public IBlingoVerboseTheOfSprite<AColor> Color => new BlingoTheTargetSprite<AColor>(_player, s => s.Color, (s, v) => s.Color = v);
        public IBlingoVerboseTheOfSprite<bool> Editable => new BlingoTheTargetSprite<bool>(_player, s => s.Editable, (s, v) => s.Editable = v);
        public IBlingoVerboseTheOfSprite<AColor> ForeColor => new BlingoTheTargetSprite<AColor>(_player, s => s.ForeColor, (s, v) => s.ForeColor = v);
        public IBlingoVerboseTheOfSprite<bool> HiliteSprite => new BlingoTheTargetSprite<bool>(_player, s => s.Hilite, (s, v) => s.Hilite = v);
        public IBlingoVerboseTheOfSprite<int> Ink => new BlingoTheTargetSprite<int>(_player, s => s.Ink, (s, v) => s.Ink = v);
        public IBlingoVerboseTheOfSprite<BlingoInkType> InkType => new BlingoTheTargetSprite<BlingoInkType>(_player, s => s.InkType, (s, v) => s.InkType = v);
        public IBlingoVerboseTheOfSprite<bool> Linked => new BlingoTheTargetSprite<bool>(_player, s => s.Linked, (s, v) => { });
        public IBlingoVerboseTheOfSprite<bool> Loaded => new BlingoTheTargetSprite<bool>(_player, s => s.Loaded, (s, v) => { });
        public IBlingoVerboseTheOfSprite<byte[]> Media => new BlingoTheTargetSprite<byte[]>(_player, s => s.Media, (s, v) => s.Media = v);
        public IBlingoVerboseTheOfSprite<bool> MediaReady => new BlingoTheTargetSprite<bool>(_player, s => s.MediaReady, (s, v) => { });
        public IBlingoVerboseTheOfSprite<int> Duration => new BlingoTheTargetSprite<int>(_player, s => s.Duration, (s, v) => { });
        public IBlingoVerboseTheOfSprite<int> CurrentTime => new BlingoTheTargetSprite<int>(_player, s => s.CurrentTime, (s, v) => s.CurrentTime = v);
        public IBlingoVerboseTheOfSprite<BlingoMediaStatus> MediaStatus => new BlingoTheTargetSprite<BlingoMediaStatus>(_player, s => s.MediaStatus, (s, v) => { });
        public IBlingoVerboseTheOfSprite<float> WidthSprite => new BlingoTheTargetSprite<float>(_player, s => s.Width, (s, v) => s.Width = v);
        public IBlingoVerboseTheOfSprite<float> HeightSprite => new BlingoTheTargetSprite<float>(_player, s => s.Height, (s, v) => s.Height = v);
        public IBlingoVerboseTheOfSprite<IBlingoMember?> Member => new BlingoTheTargetSprite<IBlingoMember?>(_player, s => s.Member, (s, v) => s.Member = v);
        public IBlingoVerboseTheOfSprite<string> ModifiedBy => new BlingoTheTargetSprite<string>(_player, s => s.ModifiedBy, (s, v) => s.ModifiedBy = v);
        public IBlingoVerboseTheOfSprite<ARect> Rect => new BlingoTheTargetSprite<ARect>(_player, s => s.Rect, (s, v) => { });
        public IBlingoVerboseTheOfSprite<APoint> RegPointSprite => new BlingoTheTargetSprite<APoint>(_player, s => s.RegPoint, (s, v) => s.RegPoint = v);
        public IBlingoVerboseTheOfSprite<APoint> Loc => new BlingoTheTargetSprite<APoint>(_player, s => s.Loc, (s, v) => s.Loc = v);
        public IBlingoVerboseTheOfSprite<float> LocH => new BlingoTheTargetSprite<float>(_player, s => s.LocH, (s, v) => s.LocH = v);
        public IBlingoVerboseTheOfSprite<float> LocV => new BlingoTheTargetSprite<float>(_player, s => s.LocV, (s, v) => s.LocV = v);
        public IBlingoVerboseTheOfSprite<int> LocZ => new BlingoTheTargetSprite<int>(_player, s => s.LocZ, (s, v) => s.LocZ = v);
        public IBlingoVerboseTheOfSprite<float> Rotation => new BlingoTheTargetSprite<float>(_player, s => s.Rotation, (s, v) => s.Rotation = v);
        public IBlingoVerboseTheOfSprite<float> Skew => new BlingoTheTargetSprite<float>(_player, s => s.Skew, (s, v) => s.Skew = v);
        public IBlingoVerboseTheOfSprite<bool> FlipH => new BlingoTheTargetSprite<bool>(_player, s => s.FlipH, (s, v) => s.FlipH = v);
        public IBlingoVerboseTheOfSprite<bool> FlipV => new BlingoTheTargetSprite<bool>(_player, s => s.FlipV, (s, v) => s.FlipV = v);
        public IBlingoVerboseTheOfSprite<float> Top => new BlingoTheTargetSprite<float>(_player, s => s.Top, (s, v) => s.Top = v);
        public IBlingoVerboseTheOfSprite<float> Bottom => new BlingoTheTargetSprite<float>(_player, s => s.Bottom, (s, v) => s.Bottom = v);
        public IBlingoVerboseTheOfSprite<float> Left => new BlingoTheTargetSprite<float>(_player, s => s.Left, (s, v) => s.Left = v);
        public IBlingoVerboseTheOfSprite<float> Right => new BlingoTheTargetSprite<float>(_player, s => s.Right, (s, v) => s.Right = v);
        public IBlingoVerboseTheOfSprite<int> Cursor => new BlingoTheTargetSprite<int>(_player, s => s.Cursor, (s, v) => s.Cursor = v);
        public IBlingoVerboseTheOfSprite<int> Constraint => new BlingoTheTargetSprite<int>(_player, s => s.Constraint, (s, v) => s.Constraint = v);
        public IBlingoVerboseTheOfSprite<bool> DirectToStage => new BlingoTheTargetSprite<bool>(_player, s => s.DirectToStage, (s, v) => s.DirectToStage = v);
        public IBlingoVerboseTheOfSprite<List<string>> ScriptInstanceList => new BlingoTheTargetSprite<List<string>>(_player, s => s.ScriptInstanceList, (s, v) => { });
        public IBlingoVerboseTheOfSprite<int> SizeSprite => new BlingoTheTargetSprite<int>(_player, s => s.Size, (s, v) => { });
        public IBlingoVerboseTheOfSprite<byte[]> Thumbnail => new BlingoTheTargetSprite<byte[]>(_player, s => s.Thumbnail, (s, v) => s.Thumbnail = v);
        public IBlingoVerboseTheOfSprite<int> MemberNum => new BlingoTheTargetSprite<int>(_player, s => s.MemberNum, (s, v) => { });

        #endregion



        #region Members
        public IBlingoVerboseTheOfMember<int> WidthMember => new BlingoTheTargetMember<IBlingoMember, int>(_player, s => s.Width, (s, v) => s.Width = v);
        public IBlingoVerboseTheOfMember<int> HeightMember => new BlingoTheTargetMember<IBlingoMember, int>(_player, s => s.Height, (s, v) => s.Height = v);
        #endregion



        #region Text Members
        public IBlingoVerboseTheOfMember<string> Text => new BlingoTheTargetMember<IBlingoMemberTextBase, string>(_player, s => s.Text, (s, v) => s.Text = v);
        public IBlingoVerboseTheOfMember<AbstTextAlignment> Alignment => new BlingoTheTargetMember<IBlingoMemberTextBase, AbstTextAlignment>(_player, s => s.Alignment, (s, v) => s.Alignment = v);
        public IBlingoVerboseTheOfMember<bool> EditableText => new BlingoTheTargetMember<IBlingoMemberTextBase, bool>(_player, s => s.Editable, (s, v) => s.Editable = v);
        public IBlingoVerboseTheOfMember<bool> WordWrap => new BlingoTheTargetMember<IBlingoMemberTextBase, bool>(_player, s => s.WordWrap, (s, v) => s.WordWrap = v);
        public IBlingoVerboseTheOfMember<int> ScrollTop => new BlingoTheTargetMember<IBlingoMemberTextBase, int>(_player, s => s.ScrollTop, (s, v) => s.ScrollTop = v);
        public IBlingoVerboseTheOfMember<string> Font => new BlingoTheTargetMember<IBlingoMemberTextBase, string>(_player, s => s.Font, (s, v) => s.Font = v);
        public IBlingoVerboseTheOfMember<int> FontSize => new BlingoTheTargetMember<IBlingoMemberTextBase, int>(_player, s => s.FontSize, (s, v) => s.FontSize = v);
        public IBlingoVerboseTheOfMember<BlingoTextStyle> FontStyle => new BlingoTheTargetMember<IBlingoMemberTextBase, BlingoTextStyle>(_player, s => s.FontStyle, (s, v) => s.FontStyle = v);
        public IBlingoVerboseTheOfMember<AColor> ColorText => new BlingoTheTargetMember<IBlingoMemberTextBase, AColor>(_player, s => s.Color, (s, v) => s.Color = v);
        public IBlingoVerboseTheOfMember<bool> Bold => new BlingoTheTargetMember<IBlingoMemberTextBase, bool>(_player, s => s.Bold, (s, v) => s.Bold = v);
        public IBlingoVerboseTheOfMember<bool> Italic => new BlingoTheTargetMember<IBlingoMemberTextBase, bool>(_player, s => s.Italic, (s, v) => s.Italic = v);
        public IBlingoVerboseTheOfMember<bool> Underline => new BlingoTheTargetMember<IBlingoMemberTextBase, bool>(_player, s => s.Underline, (s, v) => s.Underline = v);
        public IBlingoVerboseTheOfMember<int> Margin => new BlingoTheTargetMember<IBlingoMemberTextBase, int>(_player, s => s.Margin, (s, v) => s.Margin = v);
        public IBlingoVerboseTheOfMember<BlingoLines> Line => new BlingoTheTargetMember<IBlingoMemberTextBase, BlingoLines>(_player, s => s.Line, (s, v) => { });
        public IBlingoVerboseTheOfMember<BlingoWords> Word => new BlingoTheTargetMember<IBlingoMemberTextBase, BlingoWords>(_player, s => s.Word, (s, v) => { });
        public IBlingoVerboseTheOfMember<BlingoChars> Char => new BlingoTheTargetMember<IBlingoMemberTextBase, BlingoChars>(_player, s => s.Char, (s, v) => { });

        #endregion

        #region Bitmap Members
        public IBlingoVerboseTheOfMember<byte[]?> ImageDataBitmap => new BlingoTheTargetMember<BlingoMemberBitmap, byte[]?>(_player, s => s.ImageData, (s, v) => { if (v != null) s.SetImageData(v); });
        public IBlingoVerboseTheOfMember<bool> IsLoadedBitmap => new BlingoTheTargetMember<BlingoMemberBitmap, bool>(_player, s => s.IsLoaded, (s, v) => { });
        public IBlingoVerboseTheOfMember<string> FormatBitmap => new BlingoTheTargetMember<BlingoMemberBitmap, string>(_player, s => s.Format, (s, v) => { });
        #endregion

        #region Shape Members
        public IBlingoVerboseTheOfMember<BlingoList<APoint>> VertexList => new BlingoTheTargetMember<BlingoMemberShape, BlingoList<APoint>>(_player, s => s.VertexList, (s, v) => { });
        public IBlingoVerboseTheOfMember<BlingoShapeType> ShapeType => new BlingoTheTargetMember<BlingoMemberShape, BlingoShapeType>(_player, s => s.ShapeType, (s, v) => s.ShapeType = v);
        public IBlingoVerboseTheOfMember<AColor> FillColor => new BlingoTheTargetMember<BlingoMemberShape, AColor>(_player, s => s.FillColor, (s, v) => s.FillColor = v);
        public IBlingoVerboseTheOfMember<AColor> EndColor => new BlingoTheTargetMember<BlingoMemberShape, AColor>(_player, s => s.EndColor, (s, v) => s.EndColor = v);
        public IBlingoVerboseTheOfMember<AColor> StrokeColor => new BlingoTheTargetMember<BlingoMemberShape, AColor>(_player, s => s.StrokeColor, (s, v) => s.StrokeColor = v);
        public IBlingoVerboseTheOfMember<int> StrokeWidth => new BlingoTheTargetMember<BlingoMemberShape, int>(_player, s => s.StrokeWidth, (s, v) => s.StrokeWidth = v);
        public IBlingoVerboseTheOfMember<bool> Closed => new BlingoTheTargetMember<BlingoMemberShape, bool>(_player, s => s.Closed, (s, v) => s.Closed = v);
        public IBlingoVerboseTheOfMember<bool> AntiAlias => new BlingoTheTargetMember<BlingoMemberShape, bool>(_player, s => s.AntiAlias, (s, v) => s.AntiAlias = v);
        public IBlingoVerboseTheOfMember<bool> Filled => new BlingoTheTargetMember<BlingoMemberShape, bool>(_player, s => s.Filled, (s, v) => s.Filled = v);
        #endregion
    }

}




