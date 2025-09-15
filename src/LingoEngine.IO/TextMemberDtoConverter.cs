using LingoEngine.IO.Data.DTO;
using LingoEngine.Texts;

namespace LingoEngine.IO;

internal static class TextMemberDtoConverter
{
    public static LingoMemberFieldDTO ToDto(LingoMemberField field, LingoMemberDTO baseDto)
    {
        var dto = MemberDtoConverter.PopulateBase(baseDto, new LingoMemberFieldDTO());
        dto.MarkDownText = field.InitialMarkdown != null ? field.InitialMarkdown.Markdown : field.Text;
        return dto;
    }

    public static LingoMemberTextDTO ToDto(LingoMemberText text, LingoMemberDTO baseDto)
    {
        var dto = MemberDtoConverter.PopulateBase(baseDto, new LingoMemberTextDTO());
        dto.MarkDownText = text.InitialMarkdown != null ? text.InitialMarkdown.Markdown : text.Text;
        return dto;
    }
}
