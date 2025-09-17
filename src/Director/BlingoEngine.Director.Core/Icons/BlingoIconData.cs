using AbstUI.Primitives;
using BlingoEngine.Bitmaps;

namespace BlingoEngine.Director.Core.Icons;

/// <summary>
/// Raw icon pixel data used for rendering editor icons.
/// </summary>
public readonly record struct BlingoIconData(IAbstTexture2D Texture, int Width, int Height, APixelFormat Format);

