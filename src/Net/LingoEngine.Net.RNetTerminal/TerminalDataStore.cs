using System;
using System.Collections.Generic;
using System.Linq;
using LingoEngine.IO;
using LingoEngine.IO.Data.DTO;
using LingoEngine.Net.RNetContracts;

namespace LingoEngine.Net.RNetTerminal;

public sealed class TerminalDataStore
{
    private static readonly Lazy<TerminalDataStore> _instance = new(() => new TerminalDataStore());
    public static TerminalDataStore Instance => _instance.Value;

    private readonly List<LingoSpriteDTO> _sprites = new();
    private readonly Dictionary<string, List<LingoMemberDTO>> _casts = new();
    private int _currentFrame;
    private int? _selectedSprite;

    private TerminalDataStore()
    {
        LoadTestData();
    }

    public MovieStateDto MovieState { get; private set; } = TestMovieBuilder.BuildMovieState();

    public int StageWidth { get; private set; } = 640;

    public int StageHeight { get; private set; } = 480;

    public int FrameCount { get; private set; } = 600;

    public event Action? SpritesChanged;
    public event Action<LingoSpriteDTO>? SpriteChanged;
    public event Action? CastsChanged;
    public event Action<LingoMemberDTO>? MemberChanged;
    public event Action<int>? FrameChanged;
    public event Action<int?>? SelectedSpriteChanged;

    public IReadOnlyList<LingoSpriteDTO> GetSprites() => _sprites;

    public IReadOnlyDictionary<string, List<LingoMemberDTO>> GetCasts() => _casts;

    public LingoSpriteDTO? FindSprite(int spriteNum)
        => _sprites.FirstOrDefault(s => s.SpriteNum == spriteNum);

    public LingoMemberDTO? FindMember(int memberNum)
        => _casts.Values.SelectMany(c => c).FirstOrDefault(m => MemberKey(m) == memberNum);

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

    public int? GetSelectedSprite() => _selectedSprite;

    public void SelectSprite(int? spriteNum)
    {
        if (_selectedSprite == spriteNum)
        {
            return;
        }
        _selectedSprite = spriteNum;
        SelectedSpriteChanged?.Invoke(spriteNum);
    }

    public void UpdateSprite(LingoSpriteDTO sprite)
    {
        var idx = _sprites.FindIndex(s => s.SpriteNum == sprite.SpriteNum);
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
        MemberChanged?.Invoke(member);
    }

    public void LoadTestData()
    {
        MovieState = TestMovieBuilder.BuildMovieState();
        _sprites.Clear();
        _sprites.AddRange(TestMovieBuilder.BuildSprites());
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
    }

    public void LoadFromProject(LingoProjectDTO project)
    {
        if (project.Movies.Count > 0)
        {
            var movie = project.Movies[0];
            _sprites.Clear();
            _sprites.AddRange(movie.Sprites);
            _casts.Clear();
            foreach (var cast in movie.Casts)
            {
                _casts[cast.Name] = cast.Members;
            }
            FrameCount = movie.FrameCount;
            MovieState = new MovieStateDto(0, movie.Tempo, false);
            SpritesChanged?.Invoke();
            CastsChanged?.Invoke();
        }
        if (project.Stage != null)
        {
            StageWidth = project.Stage.Width;
            StageHeight = project.Stage.Height;
        }
    }

    private static int MemberKey(LingoMemberDTO member)
        => (member.CastLibNum << 16) | member.NumberInCast;
}

