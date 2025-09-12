﻿using Godot;
using LingoEngine.Sounds;
using LingoEngine.Sprites;
using System.Threading.Tasks;

namespace LingoEngine.LGodot.Sounds
{
    public class LingoGodotMemberSound : ILingoFrameworkMemberSound, IDisposable
    {
        private LingoMemberSound _lingoMemberSound;
        public bool IsLoaded { get; private set; }
        internal AudioStream? AudioStream { get; private set; }
        /// <summary>
        ///  length of this audio stream, in seconds.
        /// </summary>
        public double Length { get; private set; }
        public string StreamName { get; private set; } = "";

        public bool Stereo { get; private set; }

#pragma warning disable CS8618 
        public LingoGodotMemberSound()
#pragma warning restore CS8618 
        {

        }

        internal void Init(LingoMemberSound memberSound)
        {
            _lingoMemberSound = memberSound;
            if (!string.IsNullOrWhiteSpace(memberSound.FileName))
                Preload();
            //_lingoMemberSound.CreationDate = new FileInfo(Format).LastWriteTime;
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
        public void ReleaseFromSprite(LingoSprite2D lingoSprite) { }
        public void Preload()
        {
            if (_lingoMemberSound == null) return;// need to be initialized first
            if (IsLoaded) return;
            IsLoaded = true;
            AudioStream = GD.Load<AudioStream>($"res://{_lingoMemberSound.FileName}");
            //if (AudioStream.ok != Error.Ok)
            //{
            //    GD.PrintErr("Failed to load image data.:" + _lingoMemberPicture.FileName + ":" + error);
            //    return;
            //}
            Length = AudioStream.GetLength();
            StreamName = AudioStream._GetStreamName();
            Stereo = !AudioStream.IsMonophonic();
            _lingoMemberSound.Size = (long)AudioStream._GetLength() * 44100;
        }

        public Task PreloadAsync()
        {
            Preload();
            return Task.CompletedTask;
        }

        public void Unload()
        {
            if (!IsLoaded) return;
            _lingoMemberSound.Size = 0;
            StreamName = "";
            Length = 0;
            AudioStream?.Dispose();
            IsLoaded = false;
        }

        public bool IsPixelTransparent(int x, int y) => false;
    }
}
