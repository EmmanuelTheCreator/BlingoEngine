using LingoEngine.Members;
using LingoEngine.Sprites;
using System.Threading.Tasks;

namespace LingoEngine.ColorPalettes
{
    public class LingoColorPaletteMemberFW : ILingoFrameworkMember
    {
        public LingoColorPaletteMemberFW()
        {
        }

        public bool IsLoaded => true;

        public void CopyToClipboard()
        {

        }

        public void Erase()
        {

        }

        public void ImportFileInto()
        {

        }

        public bool IsPixelTransparent(int x, int y) => false;

        public void PasteClipboardInto()
        {

        }

        public void Preload()
        {
            if (IsLoaded)
                return;
        }

        public Task PreloadAsync()
        {
            Preload();
            return Task.CompletedTask;
        }

        public void ReleaseFromSprite(LingoSprite2D lingoSprite)
        {

        }

        public void Unload()
        {

        }
    }
}
