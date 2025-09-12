using FluentAssertions;
using LingoEngine.IO.Data.DTO;
using LingoEngine.Director.Core.Projects;

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
}
