using LingoEngine.Director.Core.Inspector;
using LingoEngine.Director.Core.UI;
using LingoEngine.Director.Core.Windowing;
using LingoEngine.FrameworkCommunication;
using Microsoft.Extensions.Logging;
using ProjectorRays.Common;
using ProjectorRays.director.Scores;
using ProjectorRays.Director;
using System;
using System.IO;
using System.Numerics;

namespace LingoEngine.Director.Core.Importer;

/// <summary>
/// Experimental binary viewer window that parses test data and exposes stream annotations.
/// </summary>
public class DirectorBinaryViewerWindowV2 : DirectorWindow<IDirFrameworkBinaryViewerWindowV2>
{
    private readonly ILogger<DirectorBinaryViewerWindowV2> _logger;

    /// <summary>Annotations gathered from the parsed score chunk.</summary>
    public RayStreamAnnotatorDecorator? InfoToShow { get; private set; }

    /// <summary>The raw bytes read from the test file.</summary>
    public byte[]? Data { get; private set; }

    public DirectorBinaryViewerWindowV2(IServiceProvider serviceProvider, ILogger<DirectorBinaryViewerWindowV2> logger, ILingoFrameworkFactory factory) : base(serviceProvider, DirectorMenuCodes.BinaryViewerWindowV2)
    {
        _logger = logger;
        Width = 1400;
        Height = 600;
        MinimumWidth = 200;
        MinimumHeight = 150;
        X = 20;
        Y = 280;
        //LoadTestData();
    }

    private void LoadTestData()
    {
        try
        {
            string path = Path.Combine(
                "..", "..", "Libraries", "LingoEngine", "WillMoveToOwnRepo", "ProjectorRays", "Test", "TestData",
                "KeyFramesTest.dir"
                );
            Data = File.ReadAllBytes(path);
            var stream = new ReadStream(Data, Data.Length, Endianness.BigEndian);
            var dir = new RaysDirectorFile(_logger);
            if (dir.Read(stream))
                InfoToShow = RaysScoreChunk.Annotator;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load test data for BinaryViewerWindowV2");
        }
    }
}
