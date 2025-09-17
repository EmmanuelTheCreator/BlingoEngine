using BlingoEngine.IO.Data.DTO.Members;
using System.Collections.Generic;

namespace BlingoEngine.IO.Data.DTO;

public class BlingoCastDTO
{
    public string Name { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public int Number { get; set; }
    public PreLoadModeTypeDTO PreLoadMode { get; set; }
    public List<BlingoMemberDTO> Members { get; set; } = new();
}

