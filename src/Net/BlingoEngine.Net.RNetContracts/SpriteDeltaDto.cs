namespace BlingoEngine.Net.RNetContracts;

/// <summary>
/// Encodes property deltas for a sprite at a given frame.
/// </summary>
/// <param name="Frame">Frame number the delta applies to.</param>
/// <param name="SpriteNum">Sprite channel number.</param>
/// <param name="BeginFrame">Sprite begin frame.</param>
/// <param name="Z">Z order.</param>
/// <param name="CastLibNum">Cast library number.</param>
/// <param name="MemberNum">Member number within the cast library.</param>
/// <param name="LocH">Horizontal position.</param>
/// <param name="LocV">Vertical position.</param>
/// <param name="Width">Width in pixels.</param>
/// <param name="Height">Height in pixels.</param>
/// <param name="Rotation">Rotation in degrees.</param>
/// <param name="Skew">Skew value.</param>
/// <param name="Blend">Blend percentage.</param>
/// <param name="Ink">Ink mode.</param>
public sealed record SpriteDeltaDto(
    int Frame,
    int SpriteNum,
    int BeginFrame,
    int Z,
    int CastLibNum,
    int MemberNum,
    int LocH,
    int LocV,
    int Width,
    int Height,
    int Rotation,
    int Skew,
    int Blend,
    int Ink) : RNetMemberRefDto(CastLibNum, MemberNum);


