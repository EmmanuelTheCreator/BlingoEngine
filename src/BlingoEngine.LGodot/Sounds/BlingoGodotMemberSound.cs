using Godot;
using BlingoEngine.Sounds;
using BlingoEngine.Sprites;
using System.Threading.Tasks;

namespace BlingoEngine.LGodot.Sounds
{
    public class BlingoGodotMemberSound : IBlingoFrameworkMemberSound, IDisposable
    {
        private BlingoMemberSound _blingoMemberSound;
        public bool IsLoaded { get; private set; }
        internal AudioStream? AudioStream { get; private set; }
        /// <summary>
        ///  length of this audio stream, in seconds.
        /// </summary>
        public double Length { get; private set; }
        public string StreamName { get; private set; } = "";

        public bool Stereo { get; private set; }

#pragma warning disable CS8618 
        public BlingoGodotMemberSound()
#pragma warning restore CS8618 
        {

        }

        internal void Init(BlingoMemberSound memberSound)
        {
            _blingoMemberSound = memberSound;
            if (!string.IsNullOrWhiteSpace(memberSound.FileName))
                Preload();
            //_blingoMemberSound.CreationDate = new FileInfo(Format).LastWriteTime;
        }

        public void Dispose()
        {
            Unload();
        }

        public void CopyToClipboard()
        {
        }

        public void Erase()
        {
        }

        public void ImportFileInto()
        {
        }

        public void PasteClipboardInto()
        {
        }
        public void ReleaseFromSprite(BlingoSprite2D blingoSprite) { }
        public void Preload()
        {
            if (_blingoMemberSound == null) return;// need to be initialized first
            if (IsLoaded) return;
            IsLoaded = true;
            AudioStream = GD.Load<AudioStream>($"res://{_blingoMemberSound.FileName}");
            //if (AudioStream.ok != Error.Ok)
            //{
            //    GD.PrintErr("Failed to load image data.:" + _blingoMemberPicture.FileName + ":" + error);
            //    return;
            //}
            Length = AudioStream.GetLength();
            StreamName = AudioStream._GetStreamName();
            Stereo = !AudioStream.IsMonophonic();
            _blingoMemberSound.Size = (long)AudioStream._GetLength() * 44100;
        }

        public Task PreloadAsync()
        {
            Preload();
            return Task.CompletedTask;
        }

        public void Unload()
        {
            if (!IsLoaded) return;
            _blingoMemberSound.Size = 0;
            StreamName = "";
            Length = 0;
            AudioStream?.Dispose();
            IsLoaded = false;
        }

        public bool IsPixelTransparent(int x, int y) => false;
    }
}

