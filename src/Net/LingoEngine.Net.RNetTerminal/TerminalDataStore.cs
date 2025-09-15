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

    private TerminalDataStore()
    {
        LoadTestData();
    }

    public MovieStateDto MovieState { get; private set; } = TestMovieBuilder.BuildMovieState();
    public IReadOnlyList<LingoSpriteDTO> Sprites { get; private set; } = TestMovieBuilder.BuildSprites();
    public Dictionary<string, List<LingoMemberDTO>> Casts { get; private set; } = TestCastBuilder.BuildCastData();
    public int StageWidth { get; private set; } = 640;
    public int StageHeight { get; private set; } = 480;
    public int FrameCount { get; private set; } = 600;

    public void LoadTestData()
    {
        MovieState = TestMovieBuilder.BuildMovieState();
        Sprites = TestMovieBuilder.BuildSprites();
        Casts = TestCastBuilder.BuildCastData();
        StageWidth = 640;
        StageHeight = 480;
        FrameCount = 600;
    }

    public void LoadFromProject(LingoProjectDTO project)
    {
        if (project.Movies.Count > 0)
        {
            var movie = project.Movies[0];
            Sprites = movie.Sprites;
            Casts = movie.Casts.ToDictionary(c => c.Name, c => c.Members);
            FrameCount = movie.FrameCount;
            MovieState = new MovieStateDto(0, movie.Tempo, false);
        }
        if (project.Stage != null)
        {
            StageWidth = project.Stage.Width;
            StageHeight = project.Stage.Height;
        }
    }
}

