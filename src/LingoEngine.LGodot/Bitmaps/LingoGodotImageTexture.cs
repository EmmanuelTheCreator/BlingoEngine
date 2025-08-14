using Godot;
using LingoEngine.Bitmaps;

namespace LingoEngine.LGodot.Bitmaps
{
   
    public class LingoGodotTexture2D : ILingoTexture2D
    {
        private readonly Texture2D _texture;
        public Texture2D Texture => _texture;

        public LingoGodotTexture2D(Texture2D imageTexture)
        {
            _texture = imageTexture;
        }

        public int Width => _texture.GetWidth();

        public int Height => _texture._GetHeight();
    }
}
