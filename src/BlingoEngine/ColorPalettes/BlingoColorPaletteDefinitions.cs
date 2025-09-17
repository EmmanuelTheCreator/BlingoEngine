
using AbstUI.Primitives;

namespace BlingoEngine.ColorPalettes
{
    public class BlingoColorPaletteDefinition
    {
        public string Name { get; private set; }
        public IEnumerable<AColor> Colors { get; private set; }

        public BlingoColorPaletteDefinition(string name, IEnumerable<AColor> colors)
        {
            Name = name;
            Colors = colors;
        }

    }

    /// <summary>
    /// Lingo Color Palette Definitions interface.
    /// </summary>
    public interface IBlingoColorPaletteDefinitions
    {
        IBlingoColorPaletteDefinitions AddPalette(string name, IEnumerable<AColor> colors);
        IEnumerable<BlingoColorPaletteDefinition> GetAll();
    }

    internal class BlingoColorPaletteDefinitions : IBlingoColorPaletteDefinitions
    {
        private readonly List<BlingoColorPaletteDefinition> _palettes = new();

        public BlingoColorPaletteDefinitions()
        {
            CreateDefaultPalettes();
        }
        public IEnumerable<BlingoColorPaletteDefinition> GetAll() => _palettes;

        public IBlingoColorPaletteDefinitions AddPalette(string name, IEnumerable<AColor> colors)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Palette name cannot be null or empty.", nameof(name));
            if (colors == null || !colors.Any())
                throw new ArgumentException("Palette must contain at least one color.", nameof(colors));
            _palettes.Add(new BlingoColorPaletteDefinition(name, colors));
            return this;
        }

        private void CreateDefaultPalettes()
        {
            AddPalette("System - Mac", BlingoColorsPalettes.SystemMacColors());
            AddPalette("System - Windows", BlingoColorsPalettes.WindowsSystemPalette());
            AddPalette("Rainbow", BlingoColorsPalettes.RainbowPalette());
            AddPalette("Grayscale", BlingoColorsPalettes.GrayscalePalette());
            AddPalette("Pastels", BlingoColorsPalettes.PastelsPalette());
            AddPalette("Vivid", BlingoColorsPalettes.VividPalette());
            AddPalette("NTSC", BlingoColorsPalettes.NTSCPalette());
            AddPalette("Metalic", BlingoColorsPalettes.MetalicPalette());
            AddPalette("Web 216", BlingoColorsPalettes.Web216Palette());
            AddPalette("System - Win (Dir 6)", BlingoColorsPalettes.WinDir4Palette());
        }







    }
}

