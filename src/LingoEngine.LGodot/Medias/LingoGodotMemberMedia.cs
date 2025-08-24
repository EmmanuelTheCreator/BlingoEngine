using Godot;
using LingoEngine.Medias;
using LingoEngine.Sprites;
using LingoEngine.Members;
using AbstUI.LGodot.Helpers;

namespace LingoEngine.LGodot.Medias
{
    public class LingoGodotMemberMedia : ILingoFrameworkMemberMedia
    {
        private LingoMemberMedia _member = null!;
        internal VideoStream? Stream { get; private set; }

        public bool IsLoaded { get; private set; }
        public int Duration { get; private set; }
        public int CurrentTime { get; set; }
        public LingoMediaStatus MediaStatus { get; private set; } = LingoMediaStatus.Closed;

        internal void Init(LingoMemberMedia member)
        {
            _member = member;
            Preload();
        }

        public void Play() => MediaStatus = LingoMediaStatus.Playing;
        public void Pause() => MediaStatus = LingoMediaStatus.Paused;
        public void Stop()
        {
            MediaStatus = LingoMediaStatus.Closed;
            CurrentTime = 0;
        }
        public void Seek(int milliseconds) => CurrentTime = milliseconds;

        public void CopyToClipboard() { }
        public void Erase() { }
        public void ImportFileInto() { }
        public void PasteClipboardInto() { }
        public void ReleaseFromSprite(LingoSprite2D lingoSprite) { }

        public void Preload()
        {
            if (IsLoaded)
                return;
            var filePath = GodotHelper.EnsureGodotUrl(_member.FileName);
            Stream = ResourceLoader.Load<VideoStream>(filePath);
            if (Stream != null)
            {
                Duration = (int)(Stream.GetLength() * 1000);
                MediaStatus = LingoMediaStatus.Opened;
            }
            IsLoaded = true;
        }

        public void Unload()
        {
            Stream = null;
            IsLoaded = false;
            MediaStatus = LingoMediaStatus.Closed;
        }

        public bool IsPixelTransparent(int x, int y) => false;
    }
}
