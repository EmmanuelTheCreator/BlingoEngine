namespace BlingoEngine.Net.RNetContracts;

/// <summary>
/// Represents a color palette used at a particular frame.
/// </summary>
/// <param name="Frame">Frame number the palette applies to.</param>
/// <param name="Argb32">ARGB palette entries.</param>
public sealed record ColorPaletteDto(int Frame, byte[] Argb32);

