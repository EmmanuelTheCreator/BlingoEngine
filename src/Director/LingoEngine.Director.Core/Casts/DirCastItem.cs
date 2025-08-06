using LingoEngine.Director.Core.Icons;
using LingoEngine.Director.Core.Styles;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Gfx;
using LingoEngine.Members;
using LingoEngine.Primitives;

namespace LingoEngine.Director.Core.Casts;

/// <summary>
/// Draws a single cast member on its own canvas. Rendering of the member
/// preview is delegated to <see cref="DirectorMemberThumbnail"/> so the logic
/// can be shared across frameworks.
/// </summary>
public class DirCastItem : DirectorMemberThumbnail
{
    public const int Width = 50;
    public const int Height = 50;
    private const int LabelHeight = 12;

    private readonly ILingoMember _member;
    private bool _selected;

    public ILingoMember Member => _member;
    public LingoGfxCanvas Canvas => base.Canvas;

    public DirCastItem(ILingoFrameworkFactory factory, ILingoMember member, int index, IDirectorIconManager? iconManager)
        : base(Width, Height, factory, iconManager)
    {
        _member = member;
        Canvas.Name = $"CastItem{index}";
        Draw();
    }

    public void SetSelected(bool selected)
    {
        _selected = selected;
        Draw();
    }

    private void Draw()
    {
        // draw member preview
        SetMember(_member);

        // label background
        Canvas.DrawRect(LingoRect.New(0, Height - LabelHeight, Width, LabelHeight),
            DirectorColors.BG_WhiteMenus, true);
        Canvas.DrawRect(LingoRect.New(0, Height - LabelHeight, Width, LabelHeight),
            LingoColorList.Gray, false);

        // label text
        string label = $"{_member.NumberInCast}. {_member.Name}";
        Canvas.DrawText(new LingoPoint(2, Height - LabelHeight), label, null,
            LingoColorList.Black, 8, Width - 4);

        // selection highlight
        if (_selected)
        {
            Canvas.DrawRect(LingoRect.New(0, 0, Width, Height),
                DirectorColors.Window_Title_BG_Active, false, 2);
        }
    }
}

