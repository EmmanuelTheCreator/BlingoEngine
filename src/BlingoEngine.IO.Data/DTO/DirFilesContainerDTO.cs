using System.Collections.Generic;

namespace BlingoEngine.IO.Data.DTO;

public class DirFilesContainerDTO
{
    public List<DirFileResourceDTO> Files { get; set; } = new();
}

