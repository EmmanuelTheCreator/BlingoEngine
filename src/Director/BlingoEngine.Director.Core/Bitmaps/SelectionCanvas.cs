using BlingoEngine.Bitmaps;
using BlingoEngine.FrameworkCommunication;
using BlingoEngine.Director.Core.Styles;
using AbstUI.Primitives;
using AbstUI.Components.Graphics;

namespace BlingoEngine.Director.Core.Bitmaps;

public class SelectionCanvas
{
    private readonly AbstGfxCanvas _canvas;
    public AbstGfxCanvas Canvas => _canvas;
    public bool Visible { get => _canvas.Visibility; set => _canvas.Visibility = value; }

    public SelectionCanvas(IBlingoFrameworkFactory factory)
    {
        _canvas = factory.CreateGfxCanvas("SelectionCanvas", 0, 0);
    }

    public void Draw(BlingoMemberBitmap member, BlingoBitmapSelection selection, float scale)
    {
        _canvas.Width = member.Width * scale;
        _canvas.Height = member.Height * scale;
        _canvas.Clear(AColors.Transparent);
        selection.Prepare((int)_canvas.Width, (int)_canvas.Height, member.Width, member.Height);
        var color = DirectorColors.BitmapSelectionFill;

        foreach (var px in selection.SelectedPixels)
        {
            var pos = selection.ToCanvas(px, scale);
            _canvas.DrawRect(ARect.New(pos.X, pos.Y, scale, scale), color, true);
        }

        if (selection.IsDragSelecting && selection.DragRect.HasValue)
        {
            var rect = selection.ToCanvas(selection.DragRect.Value, scale);
            _canvas.DrawRect(ARect.New(rect.Left, rect.Top, rect.Width, rect.Height), AColors.Cyan, false);
        }
        else if (selection.IsLassoSelecting && selection.LassoPoints.Count > 1)
        {
            for (int i = 0; i < selection.LassoPoints.Count - 1; i++)
            {
                var p1 = selection.ToCanvas(selection.LassoPoints[i], scale);
                var p2 = selection.ToCanvas(selection.LassoPoints[i + 1], scale);
                _canvas.DrawLine(p1, p2, AColors.Cyan);
            }
        }
    }
}

