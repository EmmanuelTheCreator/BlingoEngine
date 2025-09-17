using AbstUI.Primitives;
using BlingoEngine.IO.Data.DTO;

namespace BlingoEngine.IO;

internal static class ColorDtoConverter
{
    public static BlingoColorDTO ToDto(this AColor color)
    {
        return new BlingoColorDTO
        {
            Code = color.Code,
            Name = color.Name,
            R = color.R,
            G = color.G,
            B = color.B
        };
    }
}

