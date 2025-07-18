using Godot;
using LingoEngine.Sounds;

namespace LingoEngine.Director.LGodot.Scores;

internal class DirGodotScoreAudioClip
{
    internal readonly LingoMovieAudioClip Clip;
    internal bool Selected;
    internal DirGodotScoreAudioClip(LingoMovieAudioClip clip)
    {
        Clip = clip;
    }

    internal void Draw(CanvasItem canvas, Vector2 position, float width, float height, Font font)
    {
        var color = Selected ? new Color("#88ff88") : new Color("#ccffcc");
        canvas.DrawRect(new Rect2(position.X, position.Y, width, height), color);
        canvas.DrawString(font, new Vector2(position.X + 4, position.Y + font.GetAscent() - 6),
            Clip.Sound.Name ?? string.Empty, HorizontalAlignment.Left, width - 4, 9, Colors.Black);
    }
}
