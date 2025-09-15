using AbstUI.Primitives;
using LingoEngine.IO.Data.DTO;

namespace LingoEngine.IO;

internal static class ColorDtoConverter
{
    public static LingoColorDTO ToDto(AColor color)
    {
        return new LingoColorDTO
        {
            Code = color.Code,
            Name = color.Name,
            R = color.R,
            G = color.G,
            B = color.B
        };
    }
}
