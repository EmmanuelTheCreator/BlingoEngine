using AbstUI.Primitives;
using BlingoEngine.IO.Data.DTO;

namespace BlingoEngine.IO;

internal static class RectDtoConverter
{
    public static BlingoRectDTO ToDto(this ARect rect)
    {
        return new BlingoRectDTO
        {
            Left = rect.Left,
            Top = rect.Top,
            Right = rect.Right,
            Bottom = rect.Bottom
        };
    }

    public static ARect FromDto(BlingoRectDTO dto)
    {
        return new ARect(dto.Left, dto.Top, dto.Right, dto.Bottom);
    }
}

