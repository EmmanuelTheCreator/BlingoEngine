using AbstUI.Primitives;
using LingoEngine.Bitmaps;

namespace LingoEngine.Director.Core.Icons;

/// <summary>
/// Raw icon pixel data used for rendering editor icons.
/// </summary>
public readonly record struct LingoIconData(IAbstTexture2D Texture, int Width, int Height, APixelFormat Format);
