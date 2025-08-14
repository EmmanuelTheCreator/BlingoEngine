
using LingoEngine.Bitmaps;

namespace LingoEngine.Director.Core.Icons;

public interface IDirectorIconManager
{

    ILingoTexture2D Get(DirectorIcon icon);
}
