using Godot;
using LingoEngine.Medias;
using LingoEngine.Sprites;
using AbstUI.LGodot.Helpers;
using System.Threading.Tasks;

namespace LingoEngine.LGodot.Medias
{
    public class LingoGodotMemberMedia : ILingoFrameworkMemberMedia, IDisposable
    {
        private LingoMemberMedia _member = null!;
        internal VideoStream? Stream { get; private set; }

        public bool IsLoaded { get; private set; }

        private VideoStreamPlayer _player;

        public int Duration { get; private set; }
        public int CurrentTime { get; set; }
        public LingoMediaStatus MediaStatus { get; private set; } = LingoMediaStatus.Closed;

        public LingoGodotMemberMedia()
        {
            _player = new VideoStreamPlayer();
        }

        internal void Init(LingoMemberMedia member)
        {
            _member = member;

            Preload();
        }

        public void Play()
        {
            if (_player.Paused)
            {
                _player.Paused = false;
            }
            else
                _player.Play();
            MediaStatus = LingoMediaStatus.Playing;
        }

        public void Pause()
        {
            _player.Paused = true;
            MediaStatus = LingoMediaStatus.Paused;
        }

        public void Stop()
        {
            _player.Stop();
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
                _player.Stream = Stream;
                double seconds = _player.GetStreamLength();
                Duration = (int)(seconds * 1000); // seconds → ms
                MediaStatus = LingoMediaStatus.Opened;
            }
            IsLoaded = true;
        }

        public Task PreloadAsync()
        {
            Preload();
            return Task.CompletedTask;
        }

        public void Unload()
        {
            Stream?.Dispose();
            Stream = null;
            IsLoaded = false;
            MediaStatus = LingoMediaStatus.Closed;
        }

        public bool IsPixelTransparent(int x, int y) => false;

        public void Dispose()
        {
            Unload();
            _player.Dispose();
        }
    }
}
