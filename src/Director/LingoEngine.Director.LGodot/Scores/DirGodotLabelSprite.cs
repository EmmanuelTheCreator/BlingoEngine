using Godot;
using LingoEngine.Movies;

namespace LingoEngine.Director.LGodot.Scores;

/// <summary>
/// Base sprite that renders a colored box with a text label.
/// </summary>
internal abstract class DirGodotLabelSprite : DirGodotBaseSprite
{
    protected virtual string Label => string.Empty;
    protected virtual Color BoxColor => new Color("#ffd080");

    internal override void Draw(CanvasItem canvas, Vector2 position, float width, float height, Font font)
    {
        canvas.DrawRect(new Rect2(position.X, position.Y, width, height), BoxColor);
        canvas.DrawString(font, new Vector2(position.X + 2, position.Y + font.GetAscent() - 6), Label,
            HorizontalAlignment.Left, -1, 9, Colors.Black);
    }
}
