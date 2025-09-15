using AbstUI.Primitives;
using LingoEngine.IO.Data.DTO;

namespace LingoEngine.IO;

internal static class RectDtoConverter
{
    public static LingoRectDTO ToDto(ARect rect)
    {
        return new LingoRectDTO
        {
            Left = rect.Left,
            Top = rect.Top,
            Right = rect.Right,
            Bottom = rect.Bottom
        };
    }

    public static ARect FromDto(LingoRectDTO dto)
    {
        return new ARect(dto.Left, dto.Top, dto.Right, dto.Bottom);
    }
}
