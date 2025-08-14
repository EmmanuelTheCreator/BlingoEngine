using LingoEngine.Bitmaps;
using LingoEngine.Primitives;

namespace LingoEngine.Director.Core.Icons;

/// <summary>
/// Raw icon pixel data used for rendering editor icons.
/// </summary>
public readonly record struct LingoIconData(ILingoTexture2D Texture, int Width, int Height, LingoPixelFormat Format);
