namespace LingoEngine.Projects;

using System;
using System.Threading.Tasks;
using LingoEngine.Core;
using LingoEngine.Setup;
using LingoEngine.Casts;
using LingoEngine.Movies;

/// <summary>
/// Lingo Project Factory interface.
/// </summary>
public interface ILingoProjectFactory
{
    void Setup(ILingoEngineRegistration engineRegistration);
    Task LoadCastLibsAsync(ILingoCastLibsContainer castlibContainer, LingoPlayer lingoPlayer);
    Task<ILingoMovie?> LoadStartupMovieAsync(ILingoServiceProvider serviceProvider, LingoPlayer lingoPlayer);
    void Run(ILingoMovie movie, bool autoPlayMovie);
}
