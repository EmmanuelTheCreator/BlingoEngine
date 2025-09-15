using LingoEngine.Bitmaps;
using LingoEngine.FilmLoops;
using LingoEngine.IO.Data.DTO;
using LingoEngine.Members;
using LingoEngine.Sounds;
using LingoEngine.Texts;

namespace LingoEngine.IO;

internal static class MemberDtoConverter
{
    public static LingoMemberDTO ToDto(ILingoMember member, JsonStateRepository.MovieStoreOptions options)
    {
        var baseDto = CreateBaseDto(member);

        return member switch
        {
            LingoMemberField field => TextMemberDtoConverter.ToDto(field, baseDto),
            LingoMemberText text => TextMemberDtoConverter.ToDto(text, baseDto),
            LingoMemberSound sound => AudioMemberDtoConverter.ToDto(sound, baseDto, options),
            LingoMemberBitmap bitmap => BitmapMemberDtoConverter.ToDto(bitmap, baseDto, options),
            LingoFilmLoopMember filmLoop => FilmLoopMemberDtoConverter.ToDto(filmLoop, baseDto),
            _ => baseDto,
        };
    }

    public static T PopulateBase<T>(LingoMemberDTO source, T target)
        where T : LingoMemberDTO
    {
        target.Name = source.Name;
        target.Number = source.Number;
        target.CastLibNum = source.CastLibNum;
        target.NumberInCast = source.NumberInCast;
        target.Type = source.Type;
        target.RegPoint = source.RegPoint;
        target.Width = source.Width;
        target.Height = source.Height;
        target.Size = source.Size;
        target.Comments = source.Comments;
        target.FileName = source.FileName;
        target.PurgePriority = source.PurgePriority;
        return target;
    }

    private static LingoMemberDTO CreateBaseDto(ILingoMember member)
    {
        return new LingoMemberDTO
        {
            Name = member.Name,
            Number = member.Number,
            CastLibNum = member.CastLibNum,
            NumberInCast = member.NumberInCast,
            Type = (LingoMemberTypeDTO)member.Type,
            RegPoint = new LingoPointDTO { X = member.RegPoint.X, Y = member.RegPoint.Y },
            Width = member.Width,
            Height = member.Height,
            Size = member.Size,
            Comments = member.Comments,
            FileName = member.FileName,
            PurgePriority = member.PurgePriority
        };
    }
}
