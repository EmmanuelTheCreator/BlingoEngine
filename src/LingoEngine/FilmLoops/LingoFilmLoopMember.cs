using LingoEngine.Casts;
using LingoEngine.Sounds;
using LingoEngine.Sprites;
using LingoEngine.Members;
using LingoEngine.Primitives;
using LingoEngine.Bitmaps;

namespace LingoEngine.FilmLoops
{
    /// <summary>
    /// Represents a film loop cast member.
    /// </summary>
    public class LingoFilmLoopMember : LingoMember
    {
        private readonly ILingoFrameworkMemberFilmLoop _frameworkFilmLoop;

        /// <summary>
        /// Timeline data describing the sprites and sounds composing this film loop.
        /// </summary>
        public LingoFilmLoop FilmLoop { get; } = new();

        /// <summary>
        /// Gets the framework specific implementation for this film loop.
        /// </summary>
        public T Framework<T>() where T : class, ILingoFrameworkMemberFilmLoop => (T)_frameworkFilmLoop;


        /// <summary>
        /// How the film loop content is framed within the sprite rectangle.
        /// Lingo: member.framing (#crop, #scale, #auto)
        /// </summary>
        public LingoFilmLoopFraming Framing
        {
            get => _frameworkFilmLoop.Framing;
            set => _frameworkFilmLoop.Framing = value;
        }

        /// <summary>
        /// Whether the film loop should restart automatically after the last frame.
        /// Lingo: sprite.loop when used with film loops
        /// </summary>
        public bool Loop
        {
            get => _frameworkFilmLoop.Loop;
            set => _frameworkFilmLoop.Loop = value;
        }

        public LingoFilmLoopMember(ILingoFrameworkMemberFilmLoop frameworkMember, LingoCast cast, int numberInCast, string name = "", string fileName = "", LingoPoint regPoint = default)
            : base(frameworkMember, LingoMemberType.FilmLoop, cast, numberInCast, name, fileName, regPoint)
        {
            _frameworkFilmLoop = frameworkMember;
        }

        public override void Preload() => _frameworkFilmLoop.Preload();
        public override void Unload() => _frameworkFilmLoop.Unload();
        public override void Erase() => _frameworkFilmLoop.Erase();
        public override void ImportFileInto() => _frameworkFilmLoop.ImportFileInto();
        public override void CopyToClipBoard() => _frameworkFilmLoop.CopyToClipboard();
        public override void PasteClipBoardInto() => _frameworkFilmLoop.PasteClipboardInto();

        /// <summary>
        /// Adds a sprite to the film loop timeline.
        /// </summary>
        /// <param name="channel">Sprite channel inside the film loop.</param>
        /// <param name="beginFrame">First frame on which the sprite is shown.</param>
        /// <param name="endFrame">Last frame on which the sprite is shown.</param>
        /// <param name="sprite">The sprite to add.</param>
        public void AddSprite(int channel, int beginFrame, int endFrame, LingoSprite2D sprite)
            => FilmLoop.AddSprite(channel, beginFrame, endFrame, sprite);

        /// <summary>
        /// Adds a sound to one of the film loop audio channels.
        /// </summary>
        /// <param name="channel">Audio channel index (1 or 2).</param>
        /// <param name="startFrame">Frame at which the sound should start.</param>
        /// <param name="sound">Sound member to play.</param>
        public void AddSound(int channel, int startFrame, LingoMemberSound sound)
            => FilmLoop.AddSound(channel, startFrame, sound);

        /// <summary>
        /// Populate the film loop timeline from a list of existing sprites.
        /// The method normalizes the channel and frame numbers so that the
        /// earliest sprite begins on frame 1 and the lowest channel becomes
        /// channel 1.
        /// </summary>
        /// <param name="sprites">Sprites to import into the film loop.</param>
        public void AddFromSprites(IEnumerable<LingoSprite2D> sprites)
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
                AddSprite(channel, begin, end, sp);
            }
        }
    }
}
