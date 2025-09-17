using System.Collections.Generic;
using BlingoEngine.IO.Data.DTO;

namespace BlingoEngine.IO.Data.DTO.Members;

public enum BlingoShapeTypeDto
{
    Rectangle = 1,
    RoundRect = 2,
    Oval = 3,
    Line = 4,
    PolyLine = 5
}
public class BlingoMemberShapeDTO : BlingoMemberDTO
{
    public BlingoColorDTO FillColor { get; set; }
    public BlingoColorDTO StrokeColor { get; set; }
    public int StrokeWidth { get; set; }
    public BlingoShapeTypeDto ShapeType { get; set; }
    public BlingoColorDTO EndColor { get; set; }
    public bool Closed { get; set; }
    public bool Filled { get; set; }
    public bool AntiAlias { get; set; }
    public List<BlingoPointDTO> VertexList { get; set; } = new();
}

