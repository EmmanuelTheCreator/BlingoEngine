using System.Collections.Generic;
using AbstUI.Primitives;
using AbstUI.Texts;
using LingoEngine.Bitmaps;
using LingoEngine.Casts;
using LingoEngine.Core;
using LingoEngine.Medias;
using LingoEngine.Members;
using LingoEngine.Primitives;
using LingoEngine.Shapes;
using LingoEngine.Texts;

namespace LingoEngine.VerboseLanguage
{
    /// <summary>
    /// the visibility of sprite 5
    /// </summary>
    public interface ILingoVerboseThe
    {
        #region Sprites

        ILingoVerboseTheOfSprite<int> BeginFrame { get; }
        ILingoVerboseTheOfSprite<int> EndFrame { get; }
        ILingoVerboseTheOfSprite<string> NameSprite { get; }
        ILingoVerboseTheOfSprite<int> SpriteNum { get; }
        ILingoVerboseTheOfSprite<bool> Visibility { get; }
        ILingoVerboseTheOfSprite<bool> Puppet { get; }
        ILingoVerboseTheOfSprite<bool> Lock { get; }
        ILingoVerboseTheOfSprite<AColor> BackColor { get; }
        ILingoVerboseTheOfSprite<float> Blend { get; }
        ILingoVerboseTheOfSprite<LingoCast?> CastSprite { get; }
        ILingoVerboseTheOfSprite<AColor> Color { get; }
        ILingoVerboseTheOfSprite<bool> Editable { get; }
        ILingoVerboseTheOfSprite<AColor> ForeColor { get; }
        ILingoVerboseTheOfSprite<bool> HiliteSprite { get; }
        ILingoVerboseTheOfSprite<int> Ink { get; }
        ILingoVerboseTheOfSprite<LingoInkType> InkType { get; }
        ILingoVerboseTheOfSprite<bool> Linked { get; }
        ILingoVerboseTheOfSprite<bool> Loaded { get; }
        ILingoVerboseTheOfSprite<byte[]> Media { get; }
        ILingoVerboseTheOfSprite<bool> MediaReady { get; }
        ILingoVerboseTheOfSprite<int> Duration { get; }
        ILingoVerboseTheOfSprite<int> CurrentTime { get; }
        ILingoVerboseTheOfSprite<LingoMediaStatus> MediaStatus { get; }
        ILingoVerboseTheOfSprite<float> WidthSprite { get; }
        ILingoVerboseTheOfSprite<float> HeightSprite { get; }
        ILingoVerboseTheOfSprite<ILingoMember?> Member { get; }
        ILingoVerboseTheOfSprite<string> ModifiedBy { get; }
        ILingoVerboseTheOfSprite<ARect> Rect { get; }
        ILingoVerboseTheOfSprite<APoint> RegPointSprite { get; }
        ILingoVerboseTheOfSprite<APoint> Loc { get; }
        ILingoVerboseTheOfSprite<float> LocH { get; }
        ILingoVerboseTheOfSprite<float> LocV { get; }
        ILingoVerboseTheOfSprite<int> LocZ { get; }
        ILingoVerboseTheOfSprite<float> Rotation { get; }
        ILingoVerboseTheOfSprite<float> Skew { get; }
        ILingoVerboseTheOfSprite<bool> FlipH { get; }
        ILingoVerboseTheOfSprite<bool> FlipV { get; }
        ILingoVerboseTheOfSprite<float> Top { get; }
        ILingoVerboseTheOfSprite<float> Bottom { get; }
        ILingoVerboseTheOfSprite<float> Left { get; }
        ILingoVerboseTheOfSprite<float> Right { get; }
        ILingoVerboseTheOfSprite<int> Cursor { get; }
        ILingoVerboseTheOfSprite<int> Constraint { get; }
        ILingoVerboseTheOfSprite<bool> DirectToStage { get; }
        ILingoVerboseTheOfSprite<List<string>> ScriptInstanceList { get; }
        ILingoVerboseTheOfSprite<int> SizeSprite { get; }
        ILingoVerboseTheOfSprite<byte[]> Thumbnail { get; }
        ILingoVerboseTheOfSprite<int> MemberNum { get; }
        #endregion

        #region Members
        ILingoVerboseTheOfMember<int> WidthMember { get; }
        ILingoVerboseTheOfMember<int> HeightMember { get; }
        #endregion

        #region Text Members
        ILingoVerboseTheOfMember<string> Text { get; }
        ILingoVerboseTheOfMember<AbstTextAlignment> Alignment { get; }
        ILingoVerboseTheOfMember<bool> EditableText { get; }
        ILingoVerboseTheOfMember<bool> WordWrap { get; }
        ILingoVerboseTheOfMember<int> ScrollTop { get; }
        ILingoVerboseTheOfMember<string> Font { get; }
        ILingoVerboseTheOfMember<int> FontSize { get; }
        ILingoVerboseTheOfMember<LingoTextStyle> FontStyle { get; }
        ILingoVerboseTheOfMember<AColor> ColorText { get; }
        ILingoVerboseTheOfMember<bool> Bold { get; }
        ILingoVerboseTheOfMember<bool> Italic { get; }
        ILingoVerboseTheOfMember<bool> Underline { get; }
        ILingoVerboseTheOfMember<int> Margin { get; }
        ILingoVerboseTheOfMember<LingoLines> Line { get; }
        ILingoVerboseTheOfMember<LingoWords> Word { get; }
        ILingoVerboseTheOfMember<LingoChars> Char { get; }
        #endregion

        #region Bitmap Members
        ILingoVerboseTheOfMember<byte[]?> ImageDataBitmap { get; }
        ILingoVerboseTheOfMember<bool> IsLoadedBitmap { get; }
        ILingoVerboseTheOfMember<string> FormatBitmap { get; }
        #endregion

        #region Shape Members
        ILingoVerboseTheOfMember<LingoList<APoint>> VertexList { get; }
        ILingoVerboseTheOfMember<LingoShapeType> ShapeType { get; }
        ILingoVerboseTheOfMember<AColor> FillColor { get; }
        ILingoVerboseTheOfMember<AColor> EndColor { get; }
        ILingoVerboseTheOfMember<AColor> StrokeColor { get; }
        ILingoVerboseTheOfMember<int> StrokeWidth { get; }
        ILingoVerboseTheOfMember<bool> Closed { get; }
        ILingoVerboseTheOfMember<bool> AntiAlias { get; }
        ILingoVerboseTheOfMember<bool> Filled { get; }
        #endregion

    }



    /// <inheritdoc/>
    public partial record LingoVerboseThe : LingoVerboseBase, ILingoVerboseThe
    {


        public LingoVerboseThe(LingoPlayer lingoPlayer)
            : base(lingoPlayer)
        {
        }


        #region Sprites

        public ILingoVerboseTheOfSprite<int> BeginFrame => new LingoTheTargetSprite<int>(_player, s => s.BeginFrame, (s, v) => s.BeginFrame = v);
        public ILingoVerboseTheOfSprite<int> EndFrame => new LingoTheTargetSprite<int>(_player, s => s.EndFrame, (s, v) => s.EndFrame = v);
        public ILingoVerboseTheOfSprite<string> NameSprite => new LingoTheTargetSprite<string>(_player, s => s.Name, (s, v) => s.Name = v);
        public ILingoVerboseTheOfSprite<int> SpriteNum => new LingoTheTargetSprite<int>(_player, s => s.SpriteNum, (s, v) => { });
        public ILingoVerboseTheOfSprite<bool> Visibility => new LingoTheTargetSprite<bool>(_player, s => s.Visibility, (s, v) => s.Visibility = v);
        public ILingoVerboseTheOfSprite<bool> Puppet => new LingoTheTargetSprite<bool>(_player, s => s.Puppet, (s, v) => s.Puppet = v);
        public ILingoVerboseTheOfSprite<bool> Lock => new LingoTheTargetSprite<bool>(_player, s => s.Lock, (s, v) => s.Lock = v);
        public ILingoVerboseTheOfSprite<AColor> BackColor => new LingoTheTargetSprite<AColor>(_player, s => s.BackColor, (s, v) => s.BackColor = v);
        public ILingoVerboseTheOfSprite<float> Blend => new LingoTheTargetSprite<float>(_player, s => s.Blend, (s, v) => s.Blend = v);
        public ILingoVerboseTheOfSprite<LingoCast?> CastSprite => new LingoTheTargetSprite<LingoCast?>(_player, s => s.Cast, (s, v) => { });
        public ILingoVerboseTheOfSprite<AColor> Color => new LingoTheTargetSprite<AColor>(_player, s => s.Color, (s, v) => s.Color = v);
        public ILingoVerboseTheOfSprite<bool> Editable => new LingoTheTargetSprite<bool>(_player, s => s.Editable, (s, v) => s.Editable = v);
        public ILingoVerboseTheOfSprite<AColor> ForeColor => new LingoTheTargetSprite<AColor>(_player, s => s.ForeColor, (s, v) => s.ForeColor = v);
        public ILingoVerboseTheOfSprite<bool> HiliteSprite => new LingoTheTargetSprite<bool>(_player, s => s.Hilite, (s, v) => s.Hilite = v);
        public ILingoVerboseTheOfSprite<int> Ink => new LingoTheTargetSprite<int>(_player, s => s.Ink, (s, v) => s.Ink = v);
        public ILingoVerboseTheOfSprite<LingoInkType> InkType => new LingoTheTargetSprite<LingoInkType>(_player, s => s.InkType, (s, v) => s.InkType = v);
        public ILingoVerboseTheOfSprite<bool> Linked => new LingoTheTargetSprite<bool>(_player, s => s.Linked, (s, v) => { });
        public ILingoVerboseTheOfSprite<bool> Loaded => new LingoTheTargetSprite<bool>(_player, s => s.Loaded, (s, v) => { });
        public ILingoVerboseTheOfSprite<byte[]> Media => new LingoTheTargetSprite<byte[]>(_player, s => s.Media, (s, v) => s.Media = v);
        public ILingoVerboseTheOfSprite<bool> MediaReady => new LingoTheTargetSprite<bool>(_player, s => s.MediaReady, (s, v) => { });
        public ILingoVerboseTheOfSprite<int> Duration => new LingoTheTargetSprite<int>(_player, s => s.Duration, (s, v) => { });
        public ILingoVerboseTheOfSprite<int> CurrentTime => new LingoTheTargetSprite<int>(_player, s => s.CurrentTime, (s, v) => s.CurrentTime = v);
        public ILingoVerboseTheOfSprite<LingoMediaStatus> MediaStatus => new LingoTheTargetSprite<LingoMediaStatus>(_player, s => s.MediaStatus, (s, v) => { });
        public ILingoVerboseTheOfSprite<float> WidthSprite => new LingoTheTargetSprite<float>(_player, s => s.Width, (s, v) => s.Width = v);
        public ILingoVerboseTheOfSprite<float> HeightSprite => new LingoTheTargetSprite<float>(_player, s => s.Height, (s, v) => s.Height = v);
        public ILingoVerboseTheOfSprite<ILingoMember?> Member => new LingoTheTargetSprite<ILingoMember?>(_player, s => s.Member, (s, v) => s.Member = v);
        public ILingoVerboseTheOfSprite<string> ModifiedBy => new LingoTheTargetSprite<string>(_player, s => s.ModifiedBy, (s, v) => s.ModifiedBy = v);
        public ILingoVerboseTheOfSprite<ARect> Rect => new LingoTheTargetSprite<ARect>(_player, s => s.Rect, (s, v) => { });
        public ILingoVerboseTheOfSprite<APoint> RegPointSprite => new LingoTheTargetSprite<APoint>(_player, s => s.RegPoint, (s, v) => s.RegPoint = v);
        public ILingoVerboseTheOfSprite<APoint> Loc => new LingoTheTargetSprite<APoint>(_player, s => s.Loc, (s, v) => s.Loc = v);
        public ILingoVerboseTheOfSprite<float> LocH => new LingoTheTargetSprite<float>(_player, s => s.LocH, (s, v) => s.LocH = v);
        public ILingoVerboseTheOfSprite<float> LocV => new LingoTheTargetSprite<float>(_player, s => s.LocV, (s, v) => s.LocV = v);
        public ILingoVerboseTheOfSprite<int> LocZ => new LingoTheTargetSprite<int>(_player, s => s.LocZ, (s, v) => s.LocZ = v);
        public ILingoVerboseTheOfSprite<float> Rotation => new LingoTheTargetSprite<float>(_player, s => s.Rotation, (s, v) => s.Rotation = v);
        public ILingoVerboseTheOfSprite<float> Skew => new LingoTheTargetSprite<float>(_player, s => s.Skew, (s, v) => s.Skew = v);
        public ILingoVerboseTheOfSprite<bool> FlipH => new LingoTheTargetSprite<bool>(_player, s => s.FlipH, (s, v) => s.FlipH = v);
        public ILingoVerboseTheOfSprite<bool> FlipV => new LingoTheTargetSprite<bool>(_player, s => s.FlipV, (s, v) => s.FlipV = v);
        public ILingoVerboseTheOfSprite<float> Top => new LingoTheTargetSprite<float>(_player, s => s.Top, (s, v) => s.Top = v);
        public ILingoVerboseTheOfSprite<float> Bottom => new LingoTheTargetSprite<float>(_player, s => s.Bottom, (s, v) => s.Bottom = v);
        public ILingoVerboseTheOfSprite<float> Left => new LingoTheTargetSprite<float>(_player, s => s.Left, (s, v) => s.Left = v);
        public ILingoVerboseTheOfSprite<float> Right => new LingoTheTargetSprite<float>(_player, s => s.Right, (s, v) => s.Right = v);
        public ILingoVerboseTheOfSprite<int> Cursor => new LingoTheTargetSprite<int>(_player, s => s.Cursor, (s, v) => s.Cursor = v);
        public ILingoVerboseTheOfSprite<int> Constraint => new LingoTheTargetSprite<int>(_player, s => s.Constraint, (s, v) => s.Constraint = v);
        public ILingoVerboseTheOfSprite<bool> DirectToStage => new LingoTheTargetSprite<bool>(_player, s => s.DirectToStage, (s, v) => s.DirectToStage = v);
        public ILingoVerboseTheOfSprite<List<string>> ScriptInstanceList => new LingoTheTargetSprite<List<string>>(_player, s => s.ScriptInstanceList, (s, v) => { });
        public ILingoVerboseTheOfSprite<int> SizeSprite => new LingoTheTargetSprite<int>(_player, s => s.Size, (s, v) => { });
        public ILingoVerboseTheOfSprite<byte[]> Thumbnail => new LingoTheTargetSprite<byte[]>(_player, s => s.Thumbnail, (s, v) => s.Thumbnail = v);
        public ILingoVerboseTheOfSprite<int> MemberNum => new LingoTheTargetSprite<int>(_player, s => s.MemberNum, (s, v) => { });

        #endregion



        #region Members
        public ILingoVerboseTheOfMember<int> WidthMember => new LingoTheTargetMember<ILingoMember, int>(_player, s => s.Width, (s, v) => s.Width = v);
        public ILingoVerboseTheOfMember<int> HeightMember => new LingoTheTargetMember<ILingoMember, int>(_player, s => s.Height, (s, v) => s.Height = v);
        #endregion



        #region Text Members
        public ILingoVerboseTheOfMember<string> Text => new LingoTheTargetMember<ILingoMemberTextBase, string>(_player, s => s.Text, (s, v) => s.Text = v);
        public ILingoVerboseTheOfMember<AbstTextAlignment> Alignment => new LingoTheTargetMember<ILingoMemberTextBase, AbstTextAlignment>(_player, s => s.Alignment, (s, v) => s.Alignment = v);
        public ILingoVerboseTheOfMember<bool> EditableText => new LingoTheTargetMember<ILingoMemberTextBase, bool>(_player, s => s.Editable, (s, v) => s.Editable = v);
        public ILingoVerboseTheOfMember<bool> WordWrap => new LingoTheTargetMember<ILingoMemberTextBase, bool>(_player, s => s.WordWrap, (s, v) => s.WordWrap = v);
        public ILingoVerboseTheOfMember<int> ScrollTop => new LingoTheTargetMember<ILingoMemberTextBase, int>(_player, s => s.ScrollTop, (s, v) => s.ScrollTop = v);
        public ILingoVerboseTheOfMember<string> Font => new LingoTheTargetMember<ILingoMemberTextBase, string>(_player, s => s.Font, (s, v) => s.Font = v);
        public ILingoVerboseTheOfMember<int> FontSize => new LingoTheTargetMember<ILingoMemberTextBase, int>(_player, s => s.FontSize, (s, v) => s.FontSize = v);
        public ILingoVerboseTheOfMember<LingoTextStyle> FontStyle => new LingoTheTargetMember<ILingoMemberTextBase, LingoTextStyle>(_player, s => s.FontStyle, (s, v) => s.FontStyle = v);
        public ILingoVerboseTheOfMember<AColor> ColorText => new LingoTheTargetMember<ILingoMemberTextBase, AColor>(_player, s => s.Color, (s, v) => s.Color = v);
        public ILingoVerboseTheOfMember<bool> Bold => new LingoTheTargetMember<ILingoMemberTextBase, bool>(_player, s => s.Bold, (s, v) => s.Bold = v);
        public ILingoVerboseTheOfMember<bool> Italic => new LingoTheTargetMember<ILingoMemberTextBase, bool>(_player, s => s.Italic, (s, v) => s.Italic = v);
        public ILingoVerboseTheOfMember<bool> Underline => new LingoTheTargetMember<ILingoMemberTextBase, bool>(_player, s => s.Underline, (s, v) => s.Underline = v);
        public ILingoVerboseTheOfMember<int> Margin => new LingoTheTargetMember<ILingoMemberTextBase, int>(_player, s => s.Margin, (s, v) => s.Margin = v);
        public ILingoVerboseTheOfMember<LingoLines> Line => new LingoTheTargetMember<ILingoMemberTextBase, LingoLines>(_player, s => s.Line, (s, v) => { });
        public ILingoVerboseTheOfMember<LingoWords> Word => new LingoTheTargetMember<ILingoMemberTextBase, LingoWords>(_player, s => s.Word, (s, v) => { });
        public ILingoVerboseTheOfMember<LingoChars> Char => new LingoTheTargetMember<ILingoMemberTextBase, LingoChars>(_player, s => s.Char, (s, v) => { });

        #endregion

        #region Bitmap Members
        public ILingoVerboseTheOfMember<byte[]?> ImageDataBitmap => new LingoTheTargetMember<LingoMemberBitmap, byte[]?>(_player, s => s.ImageData, (s, v) => { if (v != null) s.SetImageData(v); });
        public ILingoVerboseTheOfMember<bool> IsLoadedBitmap => new LingoTheTargetMember<LingoMemberBitmap, bool>(_player, s => s.IsLoaded, (s, v) => { });
        public ILingoVerboseTheOfMember<string> FormatBitmap => new LingoTheTargetMember<LingoMemberBitmap, string>(_player, s => s.Format, (s, v) => { });
        #endregion

        #region Shape Members
        public ILingoVerboseTheOfMember<LingoList<APoint>> VertexList => new LingoTheTargetMember<LingoMemberShape, LingoList<APoint>>(_player, s => s.VertexList, (s, v) => { });
        public ILingoVerboseTheOfMember<LingoShapeType> ShapeType => new LingoTheTargetMember<LingoMemberShape, LingoShapeType>(_player, s => s.ShapeType, (s, v) => s.ShapeType = v);
        public ILingoVerboseTheOfMember<AColor> FillColor => new LingoTheTargetMember<LingoMemberShape, AColor>(_player, s => s.FillColor, (s, v) => s.FillColor = v);
        public ILingoVerboseTheOfMember<AColor> EndColor => new LingoTheTargetMember<LingoMemberShape, AColor>(_player, s => s.EndColor, (s, v) => s.EndColor = v);
        public ILingoVerboseTheOfMember<AColor> StrokeColor => new LingoTheTargetMember<LingoMemberShape, AColor>(_player, s => s.StrokeColor, (s, v) => s.StrokeColor = v);
        public ILingoVerboseTheOfMember<int> StrokeWidth => new LingoTheTargetMember<LingoMemberShape, int>(_player, s => s.StrokeWidth, (s, v) => s.StrokeWidth = v);
        public ILingoVerboseTheOfMember<bool> Closed => new LingoTheTargetMember<LingoMemberShape, bool>(_player, s => s.Closed, (s, v) => s.Closed = v);
        public ILingoVerboseTheOfMember<bool> AntiAlias => new LingoTheTargetMember<LingoMemberShape, bool>(_player, s => s.AntiAlias, (s, v) => s.AntiAlias = v);
        public ILingoVerboseTheOfMember<bool> Filled => new LingoTheTargetMember<LingoMemberShape, bool>(_player, s => s.Filled, (s, v) => s.Filled = v);
        #endregion
    }

}



