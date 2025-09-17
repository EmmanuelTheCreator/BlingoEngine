using System.Collections.Generic;
using BlingoEngine.IO.Data.DTO;
using BlingoEngine.IO.Data.DTO.Members;

namespace BlingoEngine.IO.Data.DTO.Sprites
{
    public class BlingoSpriteBehaviorDTO : BlingoMemberDTO
    {
        public string BehaviorType { get; set; } = string.Empty;
        public List<BlingoSpriteBehaviorPropertyDTO> UserProperties { get; set; } = new();
    }
    public class BlingoSpriteBehaviorPropertyDTO
    {
        public string Key { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Value { get; set; }
        public BlingoColorDTO? ColorValue { get; set; }
    }
}

