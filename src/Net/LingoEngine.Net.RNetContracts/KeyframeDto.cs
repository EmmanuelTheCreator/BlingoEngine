namespace LingoEngine.Net.RNetContracts;

/// <summary>
/// Represents a keyframe applied to a sprite at a specific frame.
/// </summary>
/// <param name="Frame">Frame number of the keyframe.</param>
/// <param name="SpriteNum">Sprite channel number affected.</param>
/// <param name="Prop">Property changed at the keyframe.</param>
/// <param name="Value">Value of the property at the keyframe.</param>
public sealed record KeyframeDto(int Frame, int SpriteNum, string Prop, string Value);
