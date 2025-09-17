using System;
using AbstUI.Primitives;
using BlingoEngine.IO.Data.DTO;
using BlingoEngine.IO.Data.DTO.Members;
using BlingoEngine.Shapes;

namespace BlingoEngine.IO
{
    public static class ShapeDtoConverter
    {
        public static BlingoMemberShapeDTO ToDto(this BlingoMemberShape field, BlingoMemberDTO baseDto)
        {
            var dto = MemberDtoConverter.PopulateBase(baseDto, new BlingoMemberShapeDTO());
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
                dto.VertexList.Add(new BlingoPointDTO { X = vertex.X, Y = vertex.Y });
            }

            return dto;
        }

        public static void Apply(BlingoMemberShapeDTO dto, BlingoMemberShape shape)
        {
            shape.FillColor = dto.FillColor.ToAColor();
            shape.EndColor = dto.EndColor.ToAColor();
            shape.StrokeColor = dto.StrokeColor.ToAColor();
            shape.StrokeWidth = dto.StrokeWidth;
            shape.Closed = dto.Closed;
            shape.Filled = dto.Filled;
            shape.AntiAlias = dto.AntiAlias;
            shape.ShapeType = dto.ShapeType.ToBlingo();

            shape.VertexList.Clear();
            if (dto.VertexList != null)
            {
                foreach (var vertex in dto.VertexList)
                {
                    shape.VertexList.Add(new APoint(vertex.X, vertex.Y));
                }
            }
        }

        public static BlingoShapeTypeDto ToDto(this BlingoShapeType shapeType)
        {
            return shapeType switch
            {
                BlingoShapeType.Rectangle => BlingoShapeTypeDto.Rectangle,
                BlingoShapeType.RoundRect => BlingoShapeTypeDto.RoundRect,
                BlingoShapeType.Oval => BlingoShapeTypeDto.Oval,
                BlingoShapeType.Line => BlingoShapeTypeDto.Line,
                BlingoShapeType.PolyLine => BlingoShapeTypeDto.PolyLine,
                _ => throw new ArgumentOutOfRangeException(nameof(shapeType), $"Not expected shape type value: {shapeType}"),
            };
        }
        public static BlingoShapeType ToBlingo(this BlingoShapeTypeDto shapeType)
        {
            return shapeType switch
            {
                BlingoShapeTypeDto.Rectangle => BlingoShapeType.Rectangle,
                BlingoShapeTypeDto.RoundRect => BlingoShapeType.RoundRect,
                BlingoShapeTypeDto.Oval => BlingoShapeType.Oval,
                BlingoShapeTypeDto.Line => BlingoShapeType.Line,
                BlingoShapeTypeDto.PolyLine => BlingoShapeType.PolyLine,
                _ => throw new ArgumentOutOfRangeException(nameof(shapeType), $"Not expected shape type value: {shapeType}"),
            };
        }

    }
}

