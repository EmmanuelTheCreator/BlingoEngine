
using AbstUI.Primitives;

namespace LingoEngine.ColorPalettes
{
    public class LingoColorPaletteDefinition
    {
        public string Name { get; private set; }
        public IEnumerable<AColor> Colors { get; private set; }

        public LingoColorPaletteDefinition(string name, IEnumerable<AColor> colors)
        {
            Name = name;
            Colors = colors;
        }

    }

    public interface ILingoColorPaletteDefinitions
    {
        ILingoColorPaletteDefinitions AddPalette(string name, IEnumerable<AColor> colors);
        IEnumerable<LingoColorPaletteDefinition> GetAll();
    }

    internal class LingoColorPaletteDefinitions : ILingoColorPaletteDefinitions
    {
        private readonly List<LingoColorPaletteDefinition> _palettes = new();

        public LingoColorPaletteDefinitions()
        {
            CreateDefaultPalettes();
        }
        public IEnumerable<LingoColorPaletteDefinition> GetAll() => _palettes;

        public ILingoColorPaletteDefinitions AddPalette(string name, IEnumerable<AColor> colors)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Palette name cannot be null or empty.", nameof(name));
            if (colors == null || !colors.Any())
                throw new ArgumentException("Palette must contain at least one color.", nameof(colors));
            _palettes.Add(new LingoColorPaletteDefinition(name, colors));
            return this;
        }

        private void CreateDefaultPalettes()
        {
            AddPalette("System - Mac",LingoColorsPalettes.SystemMacColors());
            AddPalette("System - Windows", LingoColorsPalettes.WindowsSystemPalette());
            AddPalette("Rainbow", LingoColorsPalettes.RainbowPalette());
            AddPalette("Grayscale", LingoColorsPalettes.GrayscalePalette());
            AddPalette("Pastels", LingoColorsPalettes.PastelsPalette());
            AddPalette("Vivid", LingoColorsPalettes.VividPalette());
            AddPalette("NTSC", LingoColorsPalettes.NTSCPalette());
            AddPalette("Metalic", LingoColorsPalettes.MetalicPalette());
            AddPalette("Web 216", LingoColorsPalettes.Web216Palette());
            AddPalette("System - Win (Dir 6)", LingoColorsPalettes.WinDir4Palette());
        }


      




    }
}
