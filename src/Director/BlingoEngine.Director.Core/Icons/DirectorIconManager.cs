using AbstUI.Primitives;
using BlingoEngine.Bitmaps;

namespace BlingoEngine.Director.Core.Icons
{
    public abstract class DirectorIconManager<TBlingoIconSheet> : IDirectorIconManager
        where TBlingoIconSheet : IBlingoIconSheet
    {

        private readonly List<TBlingoIconSheet> _sheets = new();
        private readonly Dictionary<DirectorIcon, IAbstTexture2D> _iconCache = new();


        public void LoadSheet(string path, int itemCount, int iconWidth, int iconHeight, int horizontalSpacing = 0)
        {
            var sheet = OnLoadSheet(path, itemCount, iconWidth, iconHeight, horizontalSpacing);
            if (sheet == null) return;
            _sheets.Add(sheet);
        }

        protected abstract TBlingoIconSheet? OnLoadSheet(string path, int itemCount, int iconWidth, int iconHeight, int horizontalSpacing = 0);


        public IAbstTexture2D Get(DirectorIcon icon)
        {
            if (_iconCache.TryGetValue(icon, out var cached))
                return cached;

            int index = (int)icon;
            int currentIndex = 0;
            foreach (var sheet in _sheets)
            {
                int count = sheet.IconCount;
                if (index < currentIndex + count)
                {
                    int localIndex = index - currentIndex;
                    int x = localIndex * (sheet.IconWidth + sheet.HorizontalSpacing);
                    var blingoTexture = OnGetTextureImage(sheet, x, icon);
                    if (blingoTexture == null)
                        continue;

                    _iconCache[icon] = blingoTexture;
                    return blingoTexture;
                }
                currentIndex += count;
            }

            throw new ArgumentOutOfRangeException(nameof(icon), "Icon index out of range.");
        }
        protected abstract IAbstTexture2D? OnGetTextureImage(TBlingoIconSheet sheet, int x, DirectorIcon icon);

    }
}

