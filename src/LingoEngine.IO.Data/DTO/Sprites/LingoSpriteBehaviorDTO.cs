using System.Collections.Generic;
using LingoEngine.IO.Data.DTO;
using LingoEngine.IO.Data.DTO.Members;

namespace LingoEngine.IO.Data.DTO.Sprites
{
    public class LingoSpriteBehaviorDTO : LingoMemberDTO
    {
        public string BehaviorType { get; set; } = string.Empty;
        public List<LingoSpriteBehaviorPropertyDTO> UserProperties { get; set; } = new();
    }
    public class LingoSpriteBehaviorPropertyDTO
    {
        public string Key { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Value { get; set; }
        public LingoColorDTO? ColorValue { get; set; }
    }
}
