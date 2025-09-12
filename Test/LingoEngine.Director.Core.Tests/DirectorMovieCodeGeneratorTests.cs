using FluentAssertions;
using LingoEngine.IO.Data.DTO;
using LingoEngine.Director.Core.Projects;
using System.Collections.Generic;

namespace LingoEngine.Director.Core.Tests;

public class DirectorMovieCodeGeneratorTests
{
    [Theory]
    [InlineData(LingoMemberTypeDTO.Bitmap, nameof(LingoEngine.Bitmaps.LingoMemberBitmap))]
    [InlineData(LingoMemberTypeDTO.Sound, nameof(LingoEngine.Sounds.LingoMemberSound))]
    [InlineData(LingoMemberTypeDTO.FilmLoop, nameof(LingoEngine.FilmLoops.LingoFilmLoopMember))]
    [InlineData(LingoMemberTypeDTO.Text, nameof(LingoEngine.Texts.LingoMemberText))]
    [InlineData(LingoMemberTypeDTO.Field, nameof(LingoEngine.Texts.LingoMemberField))]
    [InlineData(LingoMemberTypeDTO.Shape, nameof(LingoEngine.Shapes.LingoMemberShape))]
    [InlineData(LingoMemberTypeDTO.Script, nameof(LingoEngine.Scripts.LingoMemberScript))]
    [InlineData(LingoMemberTypeDTO.Palette, nameof(LingoEngine.ColorPalettes.LingoColorPaletteMember))]
    [InlineData(LingoMemberTypeDTO.Transition, nameof(LingoEngine.Transitions.LingoTransitionMember))]
    [InlineData((LingoMemberTypeDTO)999, nameof(LingoEngine.Members.LingoMember))]
    public void MemberClass_Returns_Expected_Type(LingoMemberTypeDTO type, string expected)
    {
        var gen = new DirectorMovieCodeGenerator();
        gen.MemberClass(type).Should().Be(expected);
    }

    private class TestGenerator : DirectorMovieCodeGenerator
    {
        public new string GenerateMember(LingoMemberDTO dto, int idx) => base.GenerateMember(dto, idx);
        public new string GenerateScoreClass(LingoMovieDTO movie) => base.GenerateScoreClass(movie);
    }

    [Fact]
    public void GenerateMember_Skips_Default_Properties()
    {
        var gen = new TestGenerator();
        var dto = new LingoMemberDTO
        {
            Type = LingoMemberTypeDTO.Bitmap,
            NumberInCast = 1,
            Name = "m",
            Width = 10
        };
        var code = gen.GenerateMember(dto, 1);
        code.Should().Contain("member1.Width = 10");
        code.Should().NotContain("member1.Size");
    }

    [Fact]
    public void GenerateScoreClass_Skips_Default_Properties()
    {
        var gen = new TestGenerator();
        var sprite = new LingoSpriteDTO { SpriteNum = 1, MemberNum = 1, BeginFrame = 0, EndFrame = 0, Ink = 1 };
        var cast = new LingoCastDTO
        {
            Name = "Main",
            Members = new List<LingoMemberDTO>
            {
                new()
                {
                    Number = 1,
                    NumberInCast = 1,
                    CastLibNum = 1,
                    Type = LingoMemberTypeDTO.Bitmap
                }
            }
        };
        var movie = new LingoMovieDTO
        {
            Sprites = new List<LingoSpriteDTO> { sprite },
            Casts = new List<LingoCastDTO> { cast }
        };
        var code = gen.GenerateScoreClass(movie);
        code.Should().Contain("s.Ink = 1");
        code.Should().NotContain("s.Puppet");
    }
}
