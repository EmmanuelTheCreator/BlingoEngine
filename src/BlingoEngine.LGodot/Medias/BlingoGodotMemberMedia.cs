using Godot;
using BlingoEngine.Medias;
using BlingoEngine.Sprites;
using AbstUI.LGodot.Helpers;
using System.Threading.Tasks;

namespace BlingoEngine.LGodot.Medias
{
    public class BlingoGodotMemberMedia : IBlingoFrameworkMemberMedia, IDisposable
    {
        private BlingoMemberMedia _member = null!;
        internal VideoStream? Stream { get; private set; }

        public bool IsLoaded { get; private set; }

        private VideoStreamPlayer _player;

        public int Duration { get; private set; }
        public int CurrentTime { get; set; }
        public BlingoMediaStatus MediaStatus { get; private set; } = BlingoMediaStatus.Closed;

        public BlingoGodotMemberMedia()
        {
            _player = new VideoStreamPlayer();
        }

        internal void Init(BlingoMemberMedia member)
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
            MediaStatus = BlingoMediaStatus.Playing;
        }

        public void Pause()
        {
            _player.Paused = true;
            MediaStatus = BlingoMediaStatus.Paused;
        }

        public void Stop()
        {
            _player.Stop();
            MediaStatus = BlingoMediaStatus.Closed;
            CurrentTime = 0;
        }
        public void Seek(int milliseconds) => CurrentTime = milliseconds;

        public void CopyToClipboard() { }
        public void Erase() { }
        public void ImportFileInto() { }
        public void PasteClipboardInto() { }
        public void ReleaseFromSprite(BlingoSprite2D blingoSprite) { }

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
                MediaStatus = BlingoMediaStatus.Opened;
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
            MediaStatus = BlingoMediaStatus.Closed;
        }

        public bool IsPixelTransparent(int x, int y) => false;

        public void Dispose()
        {
            Unload();
            _player.Dispose();
        }
    }
}

