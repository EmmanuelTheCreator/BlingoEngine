namespace LingoEngine.Director.Contracts;

/// <summary>
/// Encodes property deltas for a sprite at a given frame.
/// </summary>
/// <param name="Frame">Frame number the delta applies to.</param>
/// <param name="SpriteNum">Sprite channel number.</param>
/// <param name="Z">Z order.</param>
/// <param name="MemberId">Cast member identifier.</param>
/// <param name="LocH">Horizontal position.</param>
/// <param name="LocV">Vertical position.</param>
/// <param name="Width">Width in pixels.</param>
/// <param name="Height">Height in pixels.</param>
/// <param name="Rotation">Rotation in degrees.</param>
/// <param name="Skew">Skew value.</param>
/// <param name="Blend">Blend percentage.</param>
/// <param name="Ink">Ink mode.</param>
public sealed record SpriteDeltaDto(int Frame, int SpriteNum, int Z, int MemberId, int LocH, int LocV, int Width, int Height, int Rotation, int Skew, int Blend, int Ink);
