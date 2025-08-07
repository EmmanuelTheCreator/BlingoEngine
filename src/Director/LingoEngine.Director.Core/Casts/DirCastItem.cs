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
public class DirCastItem
{
    public const int Width = 58;
    public const int Height = 54;
    private const int LabelHeight = 12;

    private ILingoMember? _member;
    private readonly int _slotNumber;
    private bool _selected;
    private bool _hovered;
    private readonly LingoGfxCanvas _canvas;
    private DirectorMemberThumbnail _thumb;

    public ILingoMember? Member => _member;
    public LingoGfxCanvas Canvas => _canvas;

    public DirCastItem(ILingoFrameworkFactory factory, ILingoMember? member, int slotNumber, IDirectorIconManager? iconManager)
    {
        _member = member;
        _slotNumber = slotNumber;
        _canvas = factory.CreateGfxCanvas($"CastMemberCanvas_{slotNumber}", Width, Height);

        _thumb = new DirectorMemberThumbnail(Width - 2, Height - LabelHeight - 1, factory, iconManager, _canvas, 1, 1);
        Draw();
    }

    public void SetMember(ILingoMember? member)
    {
        _member = member;
        Draw();
    }

    public void SetSelected(bool selected)
    {
        _selected = selected;
        Draw();
    }

    public void SetHovered(bool hovered)
    {
        _hovered = hovered;
        Draw();
    }

    private void Draw()
    {
        _canvas.Clear(LingoColorList.Transparent);
        // selection highlight
        if (_selected)
        {
            _canvas.DrawRect(LingoRect.New(0, 0, Width, Height), DirectorColors.BlueSelectColor, true);
        }
        else if (_hovered)
        {
            _canvas.DrawLine(new LingoPoint(0, 0), new LingoPoint(Width, 0), LingoColorList.Black); // top
            _canvas.DrawLine(new LingoPoint(0, 0), new LingoPoint(0, Height), LingoColorList.Black); // left
            _canvas.DrawLine(new LingoPoint(0, Height), new LingoPoint(Width, Height), LingoColorList.Black); // bottom
            _canvas.DrawLine(new LingoPoint(Width, 0), new LingoPoint(Width, Height), LingoColorList.Black); // right
        }
        else
        {
            _canvas.DrawLine(new LingoPoint(0, 0), new LingoPoint(Width, 0), DirectorColors.LineDarker); // top
            _canvas.DrawLine(new LingoPoint(0, 0), new LingoPoint(0, Height), DirectorColors.LineDarker); // left
            _canvas.DrawLine(new LingoPoint(0, Height), new LingoPoint(Width, Height), DirectorColors.LineLight); // bottom
            _canvas.DrawLine(new LingoPoint(Width, 0), new LingoPoint(Width, Height), DirectorColors.LineLight); // right
        }

        // draw member preview or empty slot
        if (_member != null)
        {
            _thumb.SetMember(_member);
        }
        else
        {
            _thumb.SetEmpty();
        }

        // label text
        string label;
        if (_member != null)
            label = $"{_member.NumberInCast}. {_member.Name}";
        else
            label = _slotNumber.ToString();
        _canvas.DrawText(new LingoPoint(2, Height - LabelHeight + 10), label, null, _selected ? LingoColorList.White : LingoColorList.Black, 8, Width - 4);



    }
}

