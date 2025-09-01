using Microsoft.Extensions.Logging;
using ProjectorRays.Common;
using ProjectorRays.director.Scores;
using ProjectorRays.Director;
using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace ProjectorRays.DotNet.Test.Sprites
{
    public class SpritesTests
    {
        private readonly ILogger<DirectorFileTests> _logger;

        public SpritesTests(ITestOutputHelper output)
        {
            var factory = LoggerFactory.Create(builder =>
            {
                builder.AddProvider(new XunitLoggerProvider(output));
            });

            _logger = factory.CreateLogger<DirectorFileTests>();
        }

        [Fact]
        public void WritesScoreBytesToFile()
        {
            var path = GetPath("Sprites/5spritesTest.dir");
            var output = Path.ChangeExtension(path, ".score.txt");
            if (File.Exists(output)) File.Delete(output);

            var previous = RaysScoreChunk.FrameParserFactory;
            try
            {
                RaysScoreChunk.FrameParserFactory = (logger, annotator) => new RaysScoreFrameParserV2ToFile(logger, annotator, path);
                var data = File.ReadAllBytes(path);
                var stream = new ReadStream(data, data.Length, Endianness.BigEndian);
                var dir = new RaysDirectorFile(_logger, path);
                Assert.True(dir.Read(stream));
                Assert.True(File.Exists(output));
            }
            finally
            {
                RaysScoreChunk.FrameParserFactory = previous;
            }
        }

       


        private static string GetPath(string fileName)
        {
            var baseDir = AppContext.BaseDirectory;
            return Path.Combine(baseDir, "../../../../TestData", fileName);
        }
    }
}
