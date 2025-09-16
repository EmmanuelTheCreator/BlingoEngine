using System.Collections.Generic;
using LingoEngine.Net.RNetContracts;
using LingoEngine.IO.Data.DTO;
using LingoEngine.IO.Data.DTO.Sprites;
using LingoEngine.IO.Data.DTO.Members;

namespace LingoEngine.Net.RNetTerminal.TestData;

/// <summary>
/// Supplies sample movie and sprite data for the console client.
/// </summary>
public static class TestMovieBuilder
{
    /// <summary>
    /// Builds a sample movie state.
    /// </summary>
    public static MovieStateDto BuildMovieState() => new MovieStateDto(0, 60, false);

    /// <summary>
    /// Provides sample sprite channel data.
    /// </summary>
    public static IReadOnlyList<Lingo2DSpriteDTO> BuildSprites() => new List<Lingo2DSpriteDTO>
    {
        new() { Name = "Greeting", SpriteNum = 1, Member =new LingoMemberRefDTO{CastLibNum = 1, MemberNum = 1}, BeginFrame = 1, EndFrame = 60, LocH = 100, LocV = 100 },
        new() { Name = "Info", SpriteNum = 2, Member =new LingoMemberRefDTO{CastLibNum = 1, MemberNum = 2}, BeginFrame = 5, EndFrame = 65, LocH = 300, LocV = 200 },
        new() { Name = "Box", SpriteNum = 3, Member =new LingoMemberRefDTO{CastLibNum = 1, MemberNum = 3}, BeginFrame = 10, EndFrame = 60, LocH = 400, LocV = 300 },
        new() { Name = "Greeting", SpriteNum = 4, Member =new LingoMemberRefDTO{CastLibNum = 1, MemberNum = 1}, BeginFrame = 1, EndFrame = 60, LocH = 150, LocV = 400 },
        new() { Name = "Info", SpriteNum = 5, Member =new LingoMemberRefDTO{CastLibNum = 1, MemberNum = 2}, BeginFrame = 3, EndFrame = 70, LocH = 500, LocV = 120 },
        new() { Name = "score", SpriteNum = 6, Member =new LingoMemberRefDTO{CastLibNum = 1, MemberNum = 4}, BeginFrame = 3, EndFrame = 100, LocH = 50, LocV = 50, Width = 100, Height = 20 },
        new() { Name = "Img30x80", SpriteNum = 7, Member =new LingoMemberRefDTO{CastLibNum = 1, MemberNum = 5 }, BeginFrame = 1, EndFrame = 60, LocH = 250, LocV = 350, Width = 30, Height = 80 },
    };

    public static IReadOnlyList<LingoTempoSpriteDTO> BuildTempoSprites() => new List<LingoTempoSpriteDTO>
    {
        new()
        {
            Name = "Tempo Change",
            BeginFrame = 1,
            EndFrame = 1,
            Action = LingoTempoSpriteActionDTO.ChangeTempo,
            Tempo = 60
        },
        new()
        {
            Name = "Wait",
            BeginFrame = 10,
            EndFrame = 10,
            Action = LingoTempoSpriteActionDTO.WaitSeconds,
            WaitSeconds = 1.5f
        }
    };

    public static IReadOnlyList<LingoColorPaletteSpriteDTO> BuildPaletteSprites() => new List<LingoColorPaletteSpriteDTO>
    {
        new()
        {
            Name = "Palette",
            BeginFrame = 15,
            EndFrame = 15,
            Frame = 1,
            Settings = new LingoColorPaletteFrameSettingsDTO
            {
                ColorPaletteId = 1,
                Rate = 12,
                Cycles = 2,
                Action = LingoColorPaletteActionDTO.ColorCycling,
                TransitionOption = LingoColorPaletteTransitionOptionDTO.FadeToBlack,
                CycleOption = LingoColorPaletteCycleOptionDTO.AutoReverse
            }
        }
    };

    public static IReadOnlyList<LingoTransitionSpriteDTO> BuildTransitionSprites() => new List<LingoTransitionSpriteDTO>
    {
        new()
        {
            Name = "Fade",
            BeginFrame = 3,
            EndFrame = 3,
            Member = new LingoMemberRefDTO { CastLibNum = 1, MemberNum = 6 },
            Settings = new LingoTransitionFrameSettingsDTO
            {
                TransitionId = 1,
                TransitionName = "Fade",
                Duration = 0.5f,
                Smoothness = 0.5f,
                Affects = LingoTransitionAffectsDTO.EntireStage,
                Rect = new LingoRectDTO { Left = 0, Top = 0, Right = 640, Bottom = 480 }
            }
        }
    };

    public static IReadOnlyList<LingoSpriteSoundDTO> BuildSoundSprites() => new List<LingoSpriteSoundDTO>
    {
        new()
        {
            Name = "Intro Sound",
            Channel = 1,
            BeginFrame = 1,
            EndFrame = 30,
            Member = new LingoMemberRefDTO { CastLibNum = 1, MemberNum = 7 }
        },
        new()
        {
            Name = "Effect",
            Channel = 2,
            BeginFrame = 10,
            EndFrame = 40,
            Member = new LingoMemberRefDTO { CastLibNum = 1, MemberNum = 8 }
        }
    };

}
