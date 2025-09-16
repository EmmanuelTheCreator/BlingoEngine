using LingoEngine.IO.Data.DTO.Members;
using LingoEngine.Shapes;

namespace LingoEngine.IO
{
    public static class ShapeDtoConverter
    {
        public static LingoMemberShapeDTO ToDto(this LingoMemberShape field, LingoMemberDTO baseDto)
        {
            var dto = MemberDtoConverter.PopulateBase(baseDto, new LingoMemberShapeDTO());
            dto.FillColor = field.FillColor.ToDTO();
            dto.EndColor = field.EndColor.ToDTO();
            dto.StrokeColor = field.StrokeColor.ToDTO();
            dto.StrokeWidth = field.StrokeWidth;
            dto.Closed = field.Closed;
            dto.Filled = field.Filled;
            dto.ShapeType = field.ShapeType.ToDto();
            return dto;
        }
        //public static LingoMemberShape ToLingo(this LingoMemberShapeDTO field, LingoMemberDTO baseDto)
        //{
        //    dto.FillColor = field.FillColor.ToDTO();
        //    dto.EndColor = field.EndColor.ToDTO();
        //    dto.StrokeColor = field.StrokeColor.ToDTO();
        //    dto.StrokeWidth = field.StrokeWidth;
        //    dto.Closed = field.Closed;
        //    dto.Filled = field.Filled;
        //    dto.ShapeType = field.ShapeType.ToDto();
        //    return dto;
        //}

        
        public static LingoShapeTypeDto ToDto(this LingoShapeType shapeType)
        {
            return shapeType switch
            {
                LingoShapeType.Rectangle => LingoShapeTypeDto.Rectangle,
                LingoShapeType.RoundRect => LingoShapeTypeDto.RoundRect,
                LingoShapeType.Oval => LingoShapeTypeDto.Oval,
                LingoShapeType.Line => LingoShapeTypeDto.Line,
                LingoShapeType.PolyLine => LingoShapeTypeDto.PolyLine,
                _ => throw new ArgumentOutOfRangeException(nameof(shapeType), $"Not expected shape type value: {shapeType}"),
            };
        }
        public static LingoShapeType ToLingo(this LingoShapeTypeDto shapeType)
        {
            return shapeType switch
            {
                LingoShapeTypeDto.Rectangle => LingoShapeType.Rectangle,
                LingoShapeTypeDto.RoundRect => LingoShapeType.RoundRect,
                LingoShapeTypeDto.Oval => LingoShapeType.Oval,
                LingoShapeTypeDto.Line => LingoShapeType.Line,
                LingoShapeTypeDto.PolyLine => LingoShapeType.PolyLine,
                _ => throw new ArgumentOutOfRangeException(nameof(shapeType), $"Not expected shape type value: {shapeType}"),
            };
        }

    }
}
