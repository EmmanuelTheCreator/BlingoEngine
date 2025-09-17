using BlingoEngine.Casts;
using BlingoEngine.Sounds;
using BlingoEngine.Sprites;
using BlingoEngine.Members;
using BlingoEngine.Primitives;
using BlingoEngine.Bitmaps;
using BlingoEngine.Events;
using System;
using AbstUI.Primitives;
using System.Threading.Tasks;

namespace BlingoEngine.FilmLoops
{
    /// <summary>
    /// Represents a film loop cast member.
    /// </summary>
    public class BlingoFilmLoopMember : BlingoMember, IBlingoMemberWithTexture
    {
        private readonly IBlingoFrameworkMemberFilmLoop _frameworkFilmLoop;
        private bool _isLoaded;

        /// <summary>
        /// Description of a sound placed on one of the two audio channels.
        /// </summary>
        public record SoundEntry(int Channel, int StartFrame, BlingoMemberSound Sound);

        public List<BlingoFilmLoopMemberSprite> SpriteEntries { get; } = new();
        public List<SoundEntry> SoundEntries { get; } = new();

        private int _frameCount;
        public int FrameCount
        {
            get => _frameCount;
            private set => SetProperty(ref _frameCount, value);
        }
        public IAbstTexture2D? TextureBlingo => _frameworkFilmLoop.TextureBlingo;


        /// <summary>
        /// Gets the framework specific implementation for this film loop.
        /// </summary>
        public T Framework<T>() where T : class, IBlingoFrameworkMemberFilmLoop => (T)_frameworkFilmLoop;


        /// <summary>
        /// How the film loop content is framed within the sprite rectangle.
        /// Lingo: member.framing (#crop, #scale, #auto)
        /// </summary>
        public BlingoFilmLoopFraming Framing
        {
            get => _frameworkFilmLoop.Framing;
            set
            {
                if (_frameworkFilmLoop.Framing == value)
                    return;
                _frameworkFilmLoop.Framing = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Whether the film loop should restart automatically after the last frame.
        /// Lingo: sprite.loop when used with film loops
        /// </summary>
        public bool Loop
        {
            get => _frameworkFilmLoop.Loop;
            set
            {
                if (_frameworkFilmLoop.Loop == value)
                    return;
                _frameworkFilmLoop.Loop = value;
                OnPropertyChanged();
            }
        }



        public BlingoFilmLoopMember(IBlingoFrameworkMemberFilmLoop frameworkMember, BlingoCast cast, int numberInCast, string name = "", string fileName = "", APoint regPoint = default)
            : base(frameworkMember, BlingoMemberType.FilmLoop, cast, numberInCast, name, fileName, regPoint)
        {
            _frameworkFilmLoop = frameworkMember;
        }

        internal void PrepareFilmloop()
        {
            UpdateSize();
            HasChanged = true;
        }

        public override void Preload() => PreloadAsync().GetAwaiter().GetResult();
        public override async Task PreloadAsync()
        {
            if (_isLoaded)
                return;
            await _frameworkFilmLoop.PreloadAsync();
            UpdateSize();
            _isLoaded = true;
        }
        public void UpdateSize()
        {
            var bounds = GetBoundingBox();

            // Expand dimensions to include areas positioned left or above the origin.
            float left = MathF.Min(bounds.Left, 0);
            float top = MathF.Min(bounds.Top, 0);
            float right = MathF.Max(bounds.Right, 0);
            float bottom = MathF.Max(bounds.Bottom, 0);

            Width = (int)MathF.Ceiling(right - left);
            Height = (int)MathF.Ceiling(bottom - top);
            RegPoint = new APoint(-left, -top);
        }

        public override void Unload() => _frameworkFilmLoop.Unload();
        public override void Erase() => _frameworkFilmLoop.Erase();
        public override void ImportFileInto() => _frameworkFilmLoop.ImportFileInto();
        public override void CopyToClipBoard() => _frameworkFilmLoop.CopyToClipboard();
        public override void PasteClipBoardInto() => _frameworkFilmLoop.PasteClipboardInto();




        /// <summary>
        /// Adds a sprite to the film loop timeline.
        /// </summary>
        public BlingoFilmLoopMemberSprite AddSprite(IBlingoMember member, int channel, int begin, int end, int locH = 0, int locV = 0)
         => AddSprite(new BlingoFilmLoopMemberSprite(member, channel, begin, end, locH, locV));
        public BlingoFilmLoopMemberSprite AddSprite(BlingoFilmLoopMemberSprite sprite)
        {
            SpriteEntries.Add(sprite);
            FrameCount = Math.Max(FrameCount, sprite.EndFrame);
            return sprite;
        }
        /// <summary>
        /// Adds a sound to one of the film loop audio channels.
        /// </summary>
        /// <param name="channel">Audio channel index (1 or 2).</param>
        /// <param name="startFrame">Frame at which the sound should start.</param>
        /// <param name="sound">Sound member to play.</param>
        public void AddSound(int channel, int startFrame, BlingoMemberSound sound)
        {
            SoundEntries.Add(new SoundEntry(channel, startFrame, sound));
        }

        /// <summary>
        /// Populate the film loop timeline from a list of existing sprites.
        /// The method normalizes the channel and frame numbers so that the
        /// earliest sprite begins on frame 1 and the lowest channel becomes
        /// channel 1.
        /// </summary>
        /// <param name="sprites">Sprites to import into the film loop.</param>
        public void AddFromSprites(IBlingoEventMediator eventMediator, IBlingoSpritesPlayer spritesPlayer, IEnumerable<BlingoSprite2D> sprites)
        {
            if (sprites == null)
                return;

            var list = sprites.ToList();
            if (list.Count == 0)
                return;

            int minFrame = list.Min(s => s.BeginFrame);
            int minChannel = list.Min(s => s.SpriteNum);

            foreach (var sp in list)
            {
                int channel = sp.SpriteNum - minChannel + 1;
                int begin = sp.BeginFrame - minFrame + 1;
                int end = sp.EndFrame - minFrame + 1;
                var memberSprite = new BlingoFilmLoopMemberSprite(sp, channel, begin, end);
                AddSprite(memberSprite);
            }
        }

        public ARect GetBoundingBoxForFrame(int frame) => SpriteEntries.GetBoundingBoxForFrame(frame);
        public ARect GetBoundingBox() => SpriteEntries.GetBoundingBox();

        public IAbstTexture2D? RenderToTexture(BlingoInkType ink, AColor transparentColor)
            => _frameworkFilmLoop.RenderToTexture(ink, transparentColor);
    }
}

