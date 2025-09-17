using Microsoft.Extensions.Logging.Abstractions;
using ProjectorRays.Director;
using ProjectorRays.Common;
using BlingoEngine.IO.Data.DTO;

namespace BlingoEngine.Director.Core.Importer;

/// <summary>
/// Utility to convert Director files using the ProjectorRays library into
/// BlingoEngine data transfer objects.
/// The conversion is minimal and only extracts data required to load a movie.
/// </summary>
public static class DirectorRaysImporter
{
    public static (BlingoStageDTO Stage, BlingoMovieDTO Movie, DirFilesContainerDTO Resources) ImportMovie(string filePath)
    {
        byte[] data = File.ReadAllBytes(filePath);
        var stream = new ReadStream(data, data.Length, Endianness.BigEndian);
        var dir = new RaysDirectorFile(NullLogger.Instance);
        if (!dir.Read(stream))
            throw new Exception($"Failed to read Director file '{filePath}'");

        var resources = new DirFilesContainerDTO();
        var importData = dir.ToDto(Path.GetFileNameWithoutExtension(filePath), resources);
        return (importData.Stage, importData.Movie, resources);
    }
}

