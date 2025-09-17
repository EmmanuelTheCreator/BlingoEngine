using LingoEngine.IO.Data.DTO.Members;
using LingoEngine.Texts;

namespace LingoEngine.IO;

internal static class TextMemberDtoConverter
{
    public static LingoMemberFieldDTO ToDto(this LingoMemberField field, LingoMemberDTO baseDto)
    {
        var dto = MemberDtoConverter.PopulateBase(baseDto, new LingoMemberFieldDTO());
        dto.MarkDownText = field.InitialMarkdown != null ? field.InitialMarkdown.Markdown : field.Text;
        return dto;
    }

    public static LingoMemberTextDTO ToDto(this LingoMemberText text, LingoMemberDTO baseDto)
    {
        var dto = MemberDtoConverter.PopulateBase(baseDto, new LingoMemberTextDTO());
        dto.MarkDownText = text.InitialMarkdown != null ? text.InitialMarkdown.Markdown : text.Text;
        return dto;
    }
}
