using BlingoEngine.IO.Data.DTO;
using BlingoEngine.IO.Legacy.Director;

namespace BlingoEngine.Director.Core.Importer;

/// <summary>
/// Utility to convert Director files through the BlingoEngine legacy importer into data transfer
/// objects.
/// </summary>
public static class LegacyImporter
{
    public static (BlingoStageDTO Stage, BlingoMovieDTO Movie, DirFilesContainerDTO Resources) ImportMovie(string filePath)
    {
        var importer = new BlLegacyMovieImporter();
        return importer.Import(filePath);
    }
}
