using BlingoEngine.IO.Data.DTO.Members;
using BlingoEngine.Texts;

namespace BlingoEngine.IO;

internal static class TextMemberDtoConverter
{
    public static BlingoMemberFieldDTO ToDto(this BlingoMemberField field, BlingoMemberDTO baseDto)
    {
        var dto = MemberDtoConverter.PopulateBase(baseDto, new BlingoMemberFieldDTO());
        dto.MarkDownText = field.InitialMarkdown != null ? field.InitialMarkdown.Markdown : field.Text;
        return dto;
    }

    public static BlingoMemberTextDTO ToDto(this BlingoMemberText text, BlingoMemberDTO baseDto)
    {
        var dto = MemberDtoConverter.PopulateBase(baseDto, new BlingoMemberTextDTO());
        dto.MarkDownText = text.InitialMarkdown != null ? text.InitialMarkdown.Markdown : text.Text;
        return dto;
    }
}

