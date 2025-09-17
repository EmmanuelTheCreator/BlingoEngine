using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using ProjectorRays.Common;
using ProjectorRays.Director;
using ProjectorRays.director.Chunks;
using ProjectorRays.CastMembers;
using ProjectorRays.DotNet.Test.TestData;
using Xunit;
using Xunit.Abstractions;

namespace ProjectorRays.DotNet.Test.Texts
{
    public class MultipleTextMemberTests
    {
        private readonly ILogger<MultipleTextMemberTests> _logger;

        public MultipleTextMemberTests(ITestOutputHelper output)
        {
            var factory = LoggerFactory.Create(builder =>
            {
                builder.AddProvider(new XunitLoggerProvider(output));
            });

            _logger = factory.CreateLogger<MultipleTextMemberTests>();
        }

        [Fact]
        public void CanParseMultipleXmedChunks()
        {
            // This test focuses on the core issue: when there are multiple XMED chunks,
            // each should be correctly associated with its text member
            
            // First, test with files that definitely have XMED data
            var testFiles = new[]
            {
                "Texts_Fields/Text_Hallo_fontsize14.cst",
                "Texts_Fields/Text_Multi_Line_Multi_Style.cst",
                "Texts_Fields/Text_Single_Line_Multi_Style.cst"
            };

            foreach (var testFile in testFiles)
            {
                var path = GetPath(testFile);
                if (!File.Exists(path)) continue;

                _logger.LogInformation($"Testing {testFile}");

                var data = File.ReadAllBytes(path);
                var stream = new ReadStream(data, data.Length, Endianness.BigEndian);
                var dir = new RaysDirectorFile(_logger, path);
                Assert.True(dir.Read(stream));

                // Check how many XMED chunks we have
                var xmedChunks = dir.ChunkInfos.Values.Where(c => c.FourCC == RaysDirectorFile.FOURCC('X', 'M', 'E', 'D')).ToList();
                _logger.LogInformation($"Found {xmedChunks.Count} XMED chunks");

                var cast = Assert.Single(dir.Casts);
                var textMembers = cast.Members.Values.Where(m =>
                    m.Type == RaysMemberType.TextMember || m.Type == RaysMemberType.FieldMember).ToList();

                _logger.LogInformation($"Found {textMembers.Count} text members");

                // Each text member should have decoded text if there's an XMED chunk
                foreach (var member in textMembers)
                {
                    if (xmedChunks.Count > 0)
                    {
                        Assert.NotNull(member.DecodedText);
                        Assert.False(string.IsNullOrEmpty(member.DecodedText.Text));
                        _logger.LogInformation($"Member {member.Id}: '{member.DecodedText.Text}' (styles: {member.DecodedText.Styles.Count})");
                    }
                    else
                    {
                        _logger.LogInformation($"Member {member.Id}: No XMED chunks available");
                    }
                }
            }
        }

        [Fact] 
        public void XmedReaderCanParseIndividualChunks()
        {
            // Test that the XmedReader itself can parse individual XMED chunks correctly
            var testFiles = new[]
            {
                "Texts_Fields/Text_Hallo_fontsize14.xmed.txt",
                "Texts_Fields/Text_Multi_Line_Multi_Style.xmed.txt",
                "Texts_Fields/Text_Single_Line_Multi_Style.xmed.txt"
            };

            foreach (var testFile in testFiles)
            {
                var path = GetPath(testFile);
                if (!File.Exists(path)) continue;

                _logger.LogInformation($"Testing XMED file {testFile}");

                try
                {
                    var data = TestFileReader.ReadHexFile(path);
                    var view = CreateView(data);
                    var reader = new XmedReader();
                    
                    var doc = reader.Read(view);
                    
                    _logger.LogInformation($"Parsed text: '{doc.Text}' with {doc.Runs.Count} runs and {doc.Styles.Count} styles");
                    
                    // Test should verify that we can at least parse the structure without errors
                    Assert.True(doc.Runs.Count > 0, $"Should have at least one run for file {testFile}");
                    
                    // Now with the fixed IsPrintable function, we should be able to extract text
                    Assert.False(string.IsNullOrEmpty(doc.Text), $"Text should not be empty for file {testFile}, but got: '{doc.Text}'");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to parse {testFile}: {ex.Message}");
                    throw;
                }
            }
        }

        [Fact]
        public void CanParseTetriGroundsDirectorFile()
        {
            // This test addresses the code review feedback to test with TetriGrounds.dir
            // Check if we can read from multiple casts, multiple text members
            var path = GetTetriGroundsPath();
            if (!File.Exists(path))
            {
                _logger.LogWarning($"TetriGrounds.dir not found at {path}, skipping test");
                return;
            }

            _logger.LogInformation($"Testing TetriGrounds.dir file");

            var data = File.ReadAllBytes(path);
            var stream = new ReadStream(data, data.Length, Endianness.BigEndian);
            var dir = new RaysDirectorFile(_logger, path);
            Assert.True(dir.Read(stream));

            _logger.LogInformation($"Found {dir.Casts.Count} casts");

            // Check all casts for text members
            foreach (var cast in dir.Casts)
            {
                var textMembers = cast.Members.Values.Where(m =>
                    m.Type == RaysMemberType.TextMember || m.Type == RaysMemberType.FieldMember).ToList();

                _logger.LogInformation($"Cast {cast.Name}: Found {textMembers.Count} text members");

                foreach (var member in textMembers)
                {
                    _logger.LogInformation($"Member {member.Id}: Name='{member.GetName()}', Type={member.Type}");
                    
                    // Ensure we have the member name
                    Assert.False(string.IsNullOrEmpty(member.GetName()), $"Member {member.Id} should have a name");
                    
                    // If there's decoded text, verify it has content and style information
                    if (member.DecodedText != null)
                    {
                        _logger.LogInformation($"  Text: '{member.DecodedText.Text?.Substring(0, Math.Min(50, member.DecodedText.Text?.Length ?? 0))}'");
                        _logger.LogInformation($"  Styles: {member.DecodedText.Styles.Count}, StyleDeclarations: {member.DecodedText.StyleDeclarations.Count}");
                        
                        // Ensure we have style information as well as the text content
                        Assert.True(member.DecodedText.Styles.Count > 0, $"Member {member.Id} should have style information");
                        Assert.True(member.DecodedText.StyleDeclarations.Count > 0, $"Member {member.Id} should have style declarations");
                    }
                }
            }
        }

        private static BufferView CreateView(byte[] data)
        {
            int start = Array.IndexOf(data, (byte)'D');
            while (start >= 0 && start + 3 < data.Length)
            {
                if (data[start + 1] == (byte)'E' && data[start + 2] == (byte)'M' && data[start + 3] == (byte)'X')
                    return new BufferView(data, start, data.Length - start);

                start = Array.IndexOf(data, (byte)'D', start + 1);
            }
            throw new InvalidDataException("XMED signature not found");
        }

        private static string GetPath(string fileName)
        {
            var baseDir = AppContext.BaseDirectory;
            return Path.Combine(baseDir, "../../../../TestData", fileName);
        }

        private static string GetTetriGroundsPath()
        {
            var baseDir = AppContext.BaseDirectory;
            // Navigate to the Demo/TetriGrounds directory from the test project
            return Path.Combine(baseDir, "../../../../../Demo/TetriGrounds/TetriGrounds.dir");
        }
    }
}