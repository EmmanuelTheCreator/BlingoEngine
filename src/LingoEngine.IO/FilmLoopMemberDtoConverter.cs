using LingoEngine.FilmLoops;
using LingoEngine.IO.Data.DTO;
using System.Linq;

namespace LingoEngine.IO;

internal static class FilmLoopMemberDtoConverter
{
    public static LingoMemberFilmLoopDTO ToDto(LingoFilmLoopMember filmLoop, LingoMemberDTO baseDto)
    {
        var dto = MemberDtoConverter.PopulateBase(baseDto, new LingoMemberFilmLoopDTO());
        dto.Framing = (LingoFilmLoopFramingDTO)filmLoop.Framing;
        dto.Loop = filmLoop.Loop;
        dto.FrameCount = filmLoop.FrameCount;
        dto.SpriteEntries = filmLoop.SpriteEntries.Select(SpriteDtoConverter.ToDto).ToList();
        dto.SoundEntries = filmLoop.SoundEntries.Select(e => new LingoFilmLoopSoundEntryDTO
        {
            Channel = e.Channel,
            StartFrame = e.StartFrame,
            Member = MemberRefDtoConverter.ToDto(e.Sound) ?? new LingoMemberRefDTO()
        }).ToList();
        return dto;
    }
}
