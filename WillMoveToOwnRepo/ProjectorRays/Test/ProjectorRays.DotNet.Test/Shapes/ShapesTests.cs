using FluentAssertions;
using Microsoft.Extensions.Logging;
using ProjectorRays.Common;
using ProjectorRays.Director;
using System;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace ProjectorRays.DotNet.Test.Shapes
{
    public class ShapesTests
    {
        private readonly ILogger<DirectorFileTests> _logger;
        private readonly ITestOutputHelper _output;

        public ShapesTests(ITestOutputHelper output)
        {
            _output = output;
            var factory = LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Warning);
                builder.AddProvider(new XunitLoggerProvider(output));
            });

            _logger = factory.CreateLogger<DirectorFileTests>();
        }

        [Fact]
        public void DirWithEightShapes_ParsesShapeCastMembers()
        {
            var path = GetPath("Shapes/DirWith_8_Shapes.dir");
            var data = File.ReadAllBytes(path);
            var stream = new ReadStream(data, data.Length, Endianness.BigEndian);
            var dir = new RaysDirectorFile(_logger, path);
            dir.Read(stream).Should().BeTrue();

            var allMembers = dir.Casts
                .SelectMany(c => c.Members.Values)
                .OrderBy(m => m.Id)
                .ToList();

            var shapeChunks = allMembers
                .Where(m => m.Type == RaysMemberType.ShapeMember)
                .ToList();

            var fieldMembers = allMembers
                .Where(m => m.Type == RaysMemberType.FieldMember)
                .ToList();

            fieldMembers.Should().ContainSingle();
            fieldMembers[0].Info.Should().NotBeNull();
            fieldMembers[0].Info!.Name.Should().Be("MyPolyLine");

            shapeChunks.Should().HaveCount(7);

            var expectedShapes = new[]
            {
                new ShapeExpectation(
                    Id: 256,
                    Name: "MySquare",
                    ShapeType: 1,
                    Rect: new RaysQuickDrawRect(0, 0, 57, 71),
                    PatternId: 1,
                    ForegroundColor: 255,
                    BackgroundColor: 0,
                    FillType: 1,
                    Ink: 1,
                    LineThickness: 2,
                    LineDirection: 5),
                new ShapeExpectation(
                    Id: 257,
                    Name: "MySquareRoundBorders",
                    ShapeType: 2,
                    Rect: new RaysQuickDrawRect(0, 0, 54, 65),
                    PatternId: 1,
                    ForegroundColor: 255,
                    BackgroundColor: 0,
                    FillType: 1,
                    Ink: 1,
                    LineThickness: 2,
                    LineDirection: 5),
                new ShapeExpectation(
                    Id: 258,
                    Name: "MyOvalFilled",
                    ShapeType: 3,
                    Rect: new RaysQuickDrawRect(0, 0, 59, 66),
                    PatternId: 1,
                    ForegroundColor: 255,
                    BackgroundColor: 0,
                    FillType: 1,
                    Ink: 1,
                    LineThickness: 2,
                    LineDirection: 5),
                new ShapeExpectation(
                    Id: 265,
                    Name: "MyRectangle",
                    ShapeType: 1,
                    Rect: new RaysQuickDrawRect(0, 0, 59, 72),
                    PatternId: 1,
                    ForegroundColor: 255,
                    BackgroundColor: 0,
                    FillType: 0,
                    Ink: 0,
                    LineThickness: 2,
                    LineDirection: 5),
                new ShapeExpectation(
                    Id: 266,
                    Name: "MySquareRoundBorders",
                    ShapeType: 2,
                    Rect: new RaysQuickDrawRect(0, 0, 56, 68),
                    PatternId: 1,
                    ForegroundColor: 255,
                    BackgroundColor: 0,
                    FillType: 0,
                    Ink: 0,
                    LineThickness: 2,
                    LineDirection: 5),
                new ShapeExpectation(
                    Id: 267,
                    Name: "MyOval",
                    ShapeType: 3,
                    Rect: new RaysQuickDrawRect(0, 0, 59, 70),
                    PatternId: 1,
                    ForegroundColor: 255,
                    BackgroundColor: 0,
                    FillType: 0,
                    Ink: 0,
                    LineThickness: 2,
                    LineDirection: 5),
                new ShapeExpectation(
                    Id: 268,
                    Name: "MyLine",
                    ShapeType: 4,
                    Rect: new RaysQuickDrawRect(0, 0, 39, 74),
                    PatternId: 1,
                    ForegroundColor: 255,
                    BackgroundColor: 0,
                    FillType: 1,
                    Ink: 1,
                    LineThickness: 2,
                    LineDirection: 5)
            };

            var shapesById = shapeChunks.ToDictionary(chunk => chunk.Id);

            foreach (var expected in expectedShapes)
            {
                shapesById.Should().ContainKey(expected.Id);
                var chunk = shapesById[expected.Id];

                chunk.Info.Should().NotBeNull();
                chunk.Info!.Name.Should().Be(expected.Name);
                chunk.SpecificDataLen.Should().Be(17);

                var member = chunk.Member.Should().BeOfType<RaysShapeMember>().Subject;

                member.ShapeType.Should().Be(expected.ShapeType);
                member.InitialRect.Should().Be(expected.Rect);
                member.PatternId.Should().Be(expected.PatternId);
                member.ForegroundColor.Should().Be(expected.ForegroundColor);
                member.BackgroundColor.Should().Be(expected.BackgroundColor);
                member.FillType.Should().Be(expected.FillType);
                member.Ink.Should().Be(expected.Ink);
                member.LineThickness.Should().Be(expected.LineThickness);
                member.LineDirection.Should().Be(expected.LineDirection);
                member.IsStub.Should().BeFalse();
            }
        }

        private readonly record struct ShapeExpectation(
            ushort Id,
            string Name,
            ushort ShapeType,
            RaysQuickDrawRect Rect,
            ushort PatternId,
            byte ForegroundColor,
            byte BackgroundColor,
            byte FillType,
            byte Ink,
            byte LineThickness,
            byte LineDirection);

        private static string GetPath(string fileName)
        {
            var baseDir = AppContext.BaseDirectory;
            return Path.Combine(baseDir, "../../../../TestData", fileName);
        }
    }
}
