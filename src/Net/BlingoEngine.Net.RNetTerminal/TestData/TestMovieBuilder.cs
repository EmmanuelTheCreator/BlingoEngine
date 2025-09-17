using System.Collections.Generic;
using BlingoEngine.Net.RNetContracts;
using BlingoEngine.IO.Data.DTO;
using BlingoEngine.IO.Data.DTO.Sprites;
using BlingoEngine.IO.Data.DTO.Members;

namespace BlingoEngine.Net.RNetTerminal.TestData;

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
    public static IReadOnlyList<Blingo2DSpriteDTO> BuildSprites() => new List<Blingo2DSpriteDTO>
    {
        new() { Name = "Greeting", SpriteNum = 1, Member =new BlingoMemberRefDTO{CastLibNum = 1, MemberNum = 1}, BeginFrame = 1, EndFrame = 60, LocH = 100, LocV = 100 },
        new() { Name = "Info", SpriteNum = 2, Member =new BlingoMemberRefDTO{CastLibNum = 1, MemberNum = 2}, BeginFrame = 5, EndFrame = 65, LocH = 300, LocV = 200 },
        new() { Name = "Box", SpriteNum = 3, Member =new BlingoMemberRefDTO{CastLibNum = 1, MemberNum = 3}, BeginFrame = 10, EndFrame = 60, LocH = 400, LocV = 300 },
        new() { Name = "Greeting", SpriteNum = 4, Member =new BlingoMemberRefDTO{CastLibNum = 1, MemberNum = 1}, BeginFrame = 1, EndFrame = 60, LocH = 150, LocV = 400 },
        new() { Name = "Info", SpriteNum = 5, Member =new BlingoMemberRefDTO{CastLibNum = 1, MemberNum = 2}, BeginFrame = 3, EndFrame = 70, LocH = 500, LocV = 120 },
        new() { Name = "score", SpriteNum = 6, Member =new BlingoMemberRefDTO{CastLibNum = 1, MemberNum = 4}, BeginFrame = 3, EndFrame = 100, LocH = 50, LocV = 50, Width = 100, Height = 20 },
        new() { Name = "Img30x80", SpriteNum = 7, Member =new BlingoMemberRefDTO{CastLibNum = 1, MemberNum = 5 }, BeginFrame = 1, EndFrame = 60, LocH = 250, LocV = 350, Width = 30, Height = 80 },
    };

    public static IReadOnlyList<BlingoTempoSpriteDTO> BuildTempoSprites() => new List<BlingoTempoSpriteDTO>
    {
        new()
        {
            Name = "Tempo Change",
            BeginFrame = 1,
            EndFrame = 1,
            Action = BlingoTempoSpriteActionDTO.ChangeTempo,
            Tempo = 60
        },
        new()
        {
            Name = "Wait",
            BeginFrame = 10,
            EndFrame = 10,
            Action = BlingoTempoSpriteActionDTO.WaitSeconds,
            WaitSeconds = 1.5f
        }
    };

    public static IReadOnlyList<BlingoColorPaletteSpriteDTO> BuildPaletteSprites() => new List<BlingoColorPaletteSpriteDTO>
    {
        new()
        {
            Name = "Palette",
            BeginFrame = 15,
            EndFrame = 15,
            Frame = 1,
            Settings = new BlingoColorPaletteFrameSettingsDTO
            {
                ColorPaletteId = 1,
                Rate = 12,
                Cycles = 2,
                Action = BlingoColorPaletteActionDTO.ColorCycling,
                TransitionOption = BlingoColorPaletteTransitionOptionDTO.FadeToBlack,
                CycleOption = BlingoColorPaletteCycleOptionDTO.AutoReverse
            }
        }
    };

    public static IReadOnlyList<BlingoTransitionSpriteDTO> BuildTransitionSprites() => new List<BlingoTransitionSpriteDTO>
    {
        new()
        {
            Name = "Fade",
            BeginFrame = 3,
            EndFrame = 3,
            Member = new BlingoMemberRefDTO { CastLibNum = 1, MemberNum = 6 },
            Settings = new BlingoTransitionFrameSettingsDTO
            {
                TransitionId = 1,
                TransitionName = "Fade",
                Duration = 0.5f,
                Smoothness = 0.5f,
                Affects = BlingoTransitionAffectsDTO.EntireStage,
                Rect = new BlingoRectDTO { Left = 0, Top = 0, Right = 640, Bottom = 480 }
            }
        }
    };

    public static IReadOnlyList<BlingoSpriteSoundDTO> BuildSoundSprites() => new List<BlingoSpriteSoundDTO>
    {
        new()
        {
            Name = "Intro Sound",
            Channel = 1,
            BeginFrame = 1,
            EndFrame = 30,
            Member = new BlingoMemberRefDTO { CastLibNum = 1, MemberNum = 7 }
        },
        new()
        {
            Name = "Effect",
            Channel = 2,
            BeginFrame = 10,
            EndFrame = 40,
            Member = new BlingoMemberRefDTO { CastLibNum = 1, MemberNum = 8 }
        }
    };

}

