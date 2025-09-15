using LingoEngine.IO.Data.DTO;
using LingoEngine.Net.RNetContracts;

namespace LingoEngine.Net.RNetTerminal;

public sealed class TerminalDataStore
{
    private static readonly Lazy<TerminalDataStore> _instance = new(() => new TerminalDataStore());
    public static TerminalDataStore Instance => _instance.Value;

    private readonly List<Lingo2DSpriteDTO> _sprites = new();
    private readonly List<LingoTempoSpriteDTO> _tempoSprites = new();
    private readonly List<LingoColorPaletteSpriteDTO> _paletteSprites = new();
    private readonly List<LingoTransitionSpriteDTO> _transitionSprites = new();
    private readonly List<LingoSpriteSoundDTO> _soundSprites = new();
    private readonly Dictionary<string, List<LingoMemberDTO>> _casts = new();
    private int _currentFrame;
    private SpriteRef? _selectedSprite;
    public int SpriteChannelCount = 100;
    private TerminalDataStore()
    {
    }

    public MovieStateDto MovieState { get; private set; } = new MovieStateDto(0,0,false);

    public int StageWidth { get; private set; } = 640;

    public int StageHeight { get; private set; } = 480;

    public int FrameCount { get; private set; } = 600;

    public event Action? SpritesChanged;
    public event Action<Lingo2DSpriteDTO>? SpriteChanged;
    public event Action? CastsChanged;
    public event Action<LingoMemberDTO>? MemberChanged;
    public event Action<int>? FrameChanged;
    public event Action<SpriteRef?>? SelectedSpriteChanged;

    public IReadOnlyList<Lingo2DSpriteDTO> GetSprites() => _sprites;

    public IReadOnlyDictionary<string, List<LingoMemberDTO>> GetCasts() => _casts;

    public IReadOnlyList<LingoTempoSpriteDTO> GetTempoSprites() => _tempoSprites;

    public IReadOnlyList<LingoColorPaletteSpriteDTO> GetColorPaletteSprites() => _paletteSprites;

    public IReadOnlyList<LingoTransitionSpriteDTO> GetTransitionSprites() => _transitionSprites;

    public IReadOnlyList<LingoSpriteSoundDTO> GetSoundSprites() => _soundSprites;

    public Lingo2DSpriteDTO? FindSprite(SpriteRef sprite)
        => _sprites.FirstOrDefault(s => s.SpriteNum == sprite.SpriteNum && s.BeginFrame == sprite.BeginFrame);

    public LingoMemberDTO? FindMember(int castLibNum, int numberInCast)
        => _casts.Values.SelectMany(c => c)
            .FirstOrDefault(m => m.CastLibNum == castLibNum && m.NumberInCast == numberInCast);

    public LingoMemberDTO? FindMember(int castLibNum, string name)
        => _casts.Values.SelectMany(c => c)
            .FirstOrDefault(m => m.CastLibNum == castLibNum && m.Name == name);

    public int GetFrame() => _currentFrame;

    public void SetFrame(int frame)
    {
        if (_currentFrame == frame)
        {
            return;
        }
        _currentFrame = frame;
        FrameChanged?.Invoke(frame);
    }

    public SpriteRef? GetSelectedSprite() => _selectedSprite;

    public void SelectSprite(SpriteRef? sprite)
    {
        if (_selectedSprite == sprite)
        {
            return;
        }
        _selectedSprite = sprite;
        SelectedSpriteChanged?.Invoke(sprite);
    }

    public void UpdateSprite(Lingo2DSpriteDTO sprite)
    {
        var idx = _sprites.FindIndex(s => s.SpriteNum == sprite.SpriteNum && s.BeginFrame == sprite.BeginFrame);
        if (idx >= 0)
        {
            _sprites[idx] = sprite;
            SpriteChanged?.Invoke(sprite);
        }
        else
        {
            _sprites.Add(sprite);
            SpritesChanged?.Invoke();
        }
    }

    public void UpdateMember(LingoMemberDTO member)
    {
        var list = _casts.Values.FirstOrDefault(c =>
            c.Any(m => m.CastLibNum == member.CastLibNum && m.NumberInCast == member.NumberInCast));
        if (list != null)
        {
            var idx = list.FindIndex(m => m.CastLibNum == member.CastLibNum && m.NumberInCast == member.NumberInCast);
            if (idx >= 0)
            {
                list[idx] = member;
            }
            else
            {
                list.Add(member);
            }
        }
        MemberChanged?.Invoke(member);
    }

    public void MoveSprite(SpriteRef spriteRef, int delta)
    {
        if (delta == 0)
        {
            return;
        }

        var sprite = FindSprite(spriteRef);
        if (sprite == null)
        {
            return;
        }

        var length = Math.Max(0, sprite.EndFrame - sprite.BeginFrame);
        var maxStart = Math.Max(1, FrameCount - length);
        var newBegin = Math.Clamp(sprite.BeginFrame + delta, 1, maxStart);
        var newEnd = newBegin + length;
        if (newEnd > FrameCount)
        {
            newEnd = FrameCount;
            newBegin = Math.Max(1, newEnd - length);
        }

        if (newBegin == sprite.BeginFrame && newEnd == sprite.EndFrame)
        {
            return;
        }

        sprite.BeginFrame = newBegin;
        sprite.EndFrame = newEnd;
        SpriteChanged?.Invoke(sprite);

        if (_selectedSprite.HasValue && _selectedSprite.Value.SpriteNum == sprite.SpriteNum)
        {
            SelectSprite(new SpriteRef(sprite.SpriteNum, sprite.BeginFrame));
        }
    }

    public void PropertyHasChanged(PropertyTarget target, string name, string value, LingoMemberDTO? member = null)
    {
        if (target == PropertyTarget.Sprite)
        {
            if (!_selectedSprite.HasValue)
            {
                return;
            }

            var sprite = FindSprite(_selectedSprite.Value);
            if (sprite == null)
            {
                return;
            }

            switch (name)
            {
                case "LocH" when float.TryParse(value, out var locH):
                    sprite.LocH = locH;
                    break;
                case "LocV" when float.TryParse(value, out var locV):
                    sprite.LocV = locV;
                    break;
                case "Width" when float.TryParse(value, out var width):
                    sprite.Width = width;
                    break;
                case "Height" when float.TryParse(value, out var height):
                    sprite.Height = height;
                    break;
            }

            UpdateSprite(sprite);
        }
        else if (target == PropertyTarget.Member && member != null)
        {
            switch (name)
            {
                case "Name":
                    member.Name = value;
                    break;
                case "Comment":
                    member.Comments = value;
                    break;
            }

            UpdateMember(member);
        }
    }

    public void LoadTestData()
    {
        MovieState = TestMovieBuilder.BuildMovieState();
        _sprites.Clear();
        _sprites.AddRange(TestMovieBuilder.BuildSprites());
        _tempoSprites.Clear();
        _tempoSprites.AddRange(TestMovieBuilder.BuildTempoSprites());
        _paletteSprites.Clear();
        _paletteSprites.AddRange(TestMovieBuilder.BuildPaletteSprites());
        _transitionSprites.Clear();
        _transitionSprites.AddRange(TestMovieBuilder.BuildTransitionSprites());
        _soundSprites.Clear();
        _soundSprites.AddRange(TestMovieBuilder.BuildSoundSprites());
        _casts.Clear();
        foreach (var kv in TestCastBuilder.BuildCastData())
        {
            _casts[kv.Key] = kv.Value;
        }
        StageWidth = 640;
        StageHeight = 480;
        FrameCount = 600;
        SpritesChanged?.Invoke();
        CastsChanged?.Invoke();
        SelectSprite(null);
    }

    public void Load(LingoMovieDTO movie)
    {
        _sprites.Clear();
        _sprites.AddRange(movie.Sprite2Ds);
        _tempoSprites.Clear();
        _tempoSprites.AddRange(movie.TempoSprites);
        _paletteSprites.Clear();
        _paletteSprites.AddRange(movie.ColorPaletteSprites);
        _transitionSprites.Clear();
        _transitionSprites.AddRange(movie.TransitionSprites);
        _soundSprites.Clear();
        _soundSprites.AddRange(movie.SoundSprites);
        _casts.Clear();
        foreach (var cast in movie.Casts)
        {
            _casts[cast.Name] = cast.Members;
        }
        FrameCount = movie.FrameCount;
        SpriteChannelCount = movie.MaxSpriteChannelCount;
        MovieState = new MovieStateDto(0, movie.Tempo, false);
        SpritesChanged?.Invoke();
        CastsChanged?.Invoke();
        SelectSprite(null);
    }
    public void LoadFromProject(LingoProjectDTO project)
    {
        if (project.Movies.Count > 0)
        {
            Load(project.Movies[0]);
        }
        if (project.Stage != null)
        {
            StageWidth = project.Stage.Width;
            StageHeight = project.Stage.Height;
        }
    }

}

