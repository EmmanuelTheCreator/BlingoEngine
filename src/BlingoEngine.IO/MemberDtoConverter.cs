using BlingoEngine.Bitmaps;
using BlingoEngine.FilmLoops;
using BlingoEngine.IO.Data.DTO;
using BlingoEngine.IO.Data.DTO.Members;
using BlingoEngine.Members;
using BlingoEngine.Shapes;
using BlingoEngine.Sounds;
using BlingoEngine.Texts;

namespace BlingoEngine.IO;

internal static class MemberDtoConverter
{
    public static BlingoMemberDTO ToDto(this IBlingoMember member, JsonStateRepository.MovieStoreOptions options)
    {
        var baseDto = CreateBaseDto(member);

        return member switch
        {
            BlingoMemberField field => field.ToDto(baseDto),
            BlingoMemberText text => text.ToDto(baseDto),
            BlingoMemberSound sound => sound.ToDto(baseDto, options),
            BlingoMemberBitmap bitmap => bitmap.ToDto(baseDto, options),
            BlingoMemberShape shape => shape.ToDto(baseDto),
            BlingoFilmLoopMember filmLoop => filmLoop.ToDto(baseDto),
            _ => baseDto,
        };
    }

    public static T PopulateBase<T>(BlingoMemberDTO source, T target)
        where T : BlingoMemberDTO
    {
        target.Name = source.Name;
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

    private static BlingoMemberDTO CreateBaseDto(IBlingoMember member)
    {
        return new BlingoMemberDTO
        {
            Name = member.Name,
            CastLibNum = member.CastLibNum,
            NumberInCast = member.NumberInCast,
            Type = (BlingoMemberTypeDTO)member.Type,
            RegPoint = new BlingoPointDTO { X = member.RegPoint.X, Y = member.RegPoint.Y },
            Width = member.Width,
            Height = member.Height,
            Size = member.Size,
            Comments = member.Comments,
            FileName = member.FileName,
            PurgePriority = member.PurgePriority
        };
    }
}

