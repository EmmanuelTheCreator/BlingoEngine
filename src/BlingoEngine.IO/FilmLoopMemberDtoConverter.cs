using BlingoEngine.FilmLoops;
using BlingoEngine.IO.Data.DTO;
using BlingoEngine.IO.Data.DTO.FilmLoops;
using BlingoEngine.IO.Data.DTO.Members;
using System.Linq;

namespace BlingoEngine.IO;

internal static class FilmLoopMemberDtoConverter
{
    public static BlingoMemberFilmLoopDTO ToDto(this BlingoFilmLoopMember filmLoop, BlingoMemberDTO baseDto)
    {
        var dto = MemberDtoConverter.PopulateBase(baseDto, new BlingoMemberFilmLoopDTO());
        dto.Framing = (BlingoFilmLoopFramingDTO)filmLoop.Framing;
        dto.Loop = filmLoop.Loop;
        dto.FrameCount = filmLoop.FrameCount;
        dto.SpriteEntries = filmLoop.SpriteEntries.Select(sprite => sprite.ToDto()).ToList();
        dto.SoundEntries = filmLoop.SoundEntries.Select(e => new BlingoFilmLoopSoundEntryDTO
        {
            Channel = e.Channel,
            StartFrame = e.StartFrame,
            Member = e.Sound.ToDto() ?? new BlingoMemberRefDTO()
        }).ToList();
        return dto;
    }
}

