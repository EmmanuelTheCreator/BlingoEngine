using AbstUI.LGodot.Bitmaps;
using AbstUI.LGodot.Helpers;
using AbstUI.Primitives;
using Godot;
using BlingoEngine.Bitmaps;
using BlingoEngine.Director.Core.Icons;
using Microsoft.Extensions.Logging;

namespace BlingoEngine.Director.LGodot.Icons
{

    public class BlingoIconSheetGodot : BlingoIconSheet<AbstGodotTexture2D>
    {
        public BlingoIconSheetGodot(AbstGodotTexture2D image, int iconWidth, int iconHeight, int horizontalSpacing, int iconCount) : base(image, iconWidth, iconHeight, horizontalSpacing, iconCount)
        {
        }
    }


    public partial class DirGodotIconManager : DirectorIconManager<BlingoIconSheetGodot>
    {
        private readonly ILogger _logger;

        public DirGodotIconManager(ILogger<DirGodotIconManager> logger)
        {
            _logger = logger;
        }
        protected override BlingoIconSheetGodot? OnLoadSheet(string path, int itemCount, int iconWidth, int iconHeight, int horizontalSpacing = 0)
        {
            string pathResource = GodotHelper.EnsureGodotUrl(path);
            var texture = GD.Load<Texture2D>(pathResource);

            if (texture == null )
            {
                _logger.LogWarning($"Failed to load texture: {path}");
                return null;
            }
            var blingoTexture = new AbstGodotTexture2D(texture);
            return new BlingoIconSheetGodot(blingoTexture,iconWidth,iconHeight,horizontalSpacing, itemCount);
        }
        protected override IAbstTexture2D? OnGetTextureImage(BlingoIconSheetGodot sheet, int x)
        {
            var texture = sheet.Image.Texture;
            var image = texture.GetImage();
            // Create an image with the correct dimensions for the icon
            var sub = Image.CreateEmpty(sheet.IconWidth, sheet.IconHeight, false, image.GetFormat());
            // Copy the icon region from the sprite sheet
            sub.BlitRect(image, new Rect2I(x, 0, sheet.IconWidth, sheet.IconHeight), Vector2I.Zero);
            var tex = ImageTexture.CreateFromImage(sub);
            var blingoTexture = new AbstGodotTexture2D(tex);
            return blingoTexture;
        }
    }
}

