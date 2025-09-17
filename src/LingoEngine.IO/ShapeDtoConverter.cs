using System;
using AbstUI.Primitives;
using LingoEngine.IO.Data.DTO;
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
            dto.AntiAlias = field.AntiAlias;
            dto.VertexList.Clear();
            foreach (var vertex in field.VertexList)
            {
                dto.VertexList.Add(new LingoPointDTO { X = vertex.X, Y = vertex.Y });
            }

            return dto;
        }

        public static void Apply(LingoMemberShapeDTO dto, LingoMemberShape shape)
        {
            shape.FillColor = dto.FillColor.ToAColor();
            shape.EndColor = dto.EndColor.ToAColor();
            shape.StrokeColor = dto.StrokeColor.ToAColor();
            shape.StrokeWidth = dto.StrokeWidth;
            shape.Closed = dto.Closed;
            shape.Filled = dto.Filled;
            shape.AntiAlias = dto.AntiAlias;
            shape.ShapeType = dto.ShapeType.ToLingo();

            shape.VertexList.Clear();
            if (dto.VertexList != null)
            {
                foreach (var vertex in dto.VertexList)
                {
                    shape.VertexList.Add(new APoint(vertex.X, vertex.Y));
                }
            }
        }

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
