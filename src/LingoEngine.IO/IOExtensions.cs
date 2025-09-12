using AbstUI.Primitives;
using LingoEngine.IO.Data.DTO;

namespace LingoEngine.IO
{
    public static class IOExtensions
    {
        public static LingoColorDTO ToDTO(this AColor color)
            => new LingoColorDTO(color.Code, color.Name, color.R, color.G, color.B, color.A);
        public static AColor ToAColor(this LingoColorDTO color)
            => new AColor(color.Code, color.R, color.G, color.B, color.Name, color.A);
    }
}
