using AbstUI.Primitives;
using BlingoEngine.IO.Data.DTO;

namespace BlingoEngine.IO
{
    public static class IOExtensions
    {
        public static BlingoColorDTO ToDTO(this AColor color)
            => new BlingoColorDTO(color.Code, color.Name, color.R, color.G, color.B, color.A);
        public static AColor ToAColor(this BlingoColorDTO color)
            => new AColor(color.Code, color.R, color.G, color.B, color.Name, color.A);
    }
}

