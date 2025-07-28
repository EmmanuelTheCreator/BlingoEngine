using Godot;
using LingoEngine.Sprites;

namespace LingoEngine.Director.LGodot.Scores;

internal class DirGodotTopSprite<TSprite> : DirGodotBaseSprite<TSprite>
    where TSprite : LingoSprite
{
    



  
    internal override void Draw(CanvasItem canvas, Vector2 position, float width, float height, Font font)
    {
        var color = Selected ? new Color("#88ff88") : new Color("#ccffcc");
        canvas.DrawRect(new Rect2(position.X, position.Y, width, height), color);
        canvas.DrawString(font, new Vector2(position.X + 4, position.Y + font.GetAscent() - 6),
            DrawLabel, HorizontalAlignment.Left, width - 4, 9, Colors.Black);
    }
}
