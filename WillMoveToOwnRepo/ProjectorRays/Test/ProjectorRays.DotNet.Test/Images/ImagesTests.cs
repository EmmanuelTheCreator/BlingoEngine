using Microsoft.Extensions.Logging;
using ProjectorRays.Common;
using ProjectorRays.director.Chunks;
using ProjectorRays.director.Scores;
using ProjectorRays.Director;
using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace ProjectorRays.DotNet.Test.Images
{
    public class ImagesTests
    {
        private readonly ILogger<DirectorFileTests> _logger;

        public ImagesTests(ITestOutputHelper output)
        {
            var factory = LoggerFactory.Create(builder =>
            {
                builder.AddProvider(new XunitLoggerProvider(output));
            });

            _logger = factory.CreateLogger<DirectorFileTests>();
        }

        [Fact]
        public void ImgCastContainsBitmapMember()
        {
            var path = GetPath("Images/ImgCast.cst");
            var data = File.ReadAllBytes(path);
            var stream = new ReadStream(data, data.Length, Endianness.BigEndian);
            var dir = new RaysDirectorFile(_logger, path);
            Assert.True(dir.Read(stream));
            const uint CASt = ((uint)'C' << 24) | ((uint)'A' << 16) | ((uint)'S' << 8) | (uint)'t';
            bool found = false;
            if (dir.Casts.Count > 0)
            {
                foreach (var id in dir.Casts[0].MemberIDs)
                {
                    var chunk = (RaysCastMemberChunk)dir.GetChunk(CASt, id);
                    if (chunk.Type == RaysMemberType.BitmapMember)
                    {
                        found = true;
                        break;
                    }
                }
            }
            Assert.True(found);
        }
        [Fact]
        public void ScoreTimelineHasSpriteFrames()
        {
            var path = GetPath("Images/Dir_With_One_Img_Sprite_Hallo.dir");
            var data = File.ReadAllBytes(path);
            var stream = new ReadStream(data, data.Length, Endianness.BigEndian);
            var dir = new RaysDirectorFile(_logger, path);
            Assert.True(dir.Read(stream));

            Assert.NotNull(dir.Score);
            Assert.NotEmpty(dir.Score!.Sprites);
            var first = dir.Score.Sprites[0];
            Assert.True(first.EndFrame >= first.StartFrame);
        }

        private static string GetPath(string fileName)
        {
            var baseDir = AppContext.BaseDirectory;
            return Path.Combine(baseDir, "../../../../TestData", fileName);
        }
    }
}
