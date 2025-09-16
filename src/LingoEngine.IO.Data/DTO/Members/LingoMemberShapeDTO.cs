namespace LingoEngine.IO.Data.DTO.Members;

public enum LingoShapeTypeDto
{
    Rectangle = 1,
    RoundRect = 2,
    Oval = 3,
    Line = 4,
    PolyLine = 5
}
public class LingoMemberShapeDTO : LingoMemberDTO
{
    public LingoColorDTO FillColor { get; set; }
    public LingoColorDTO StrokeColor { get; set; }
    public int StrokeWidth { get; set; }
    public LingoShapeTypeDto ShapeType { get; set; }
    public LingoColorDTO EndColor { get; set; }
    public bool Closed { get; set; }
    public bool Filled { get; set; }
}
