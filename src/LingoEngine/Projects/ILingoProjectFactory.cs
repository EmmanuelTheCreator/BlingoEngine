namespace LingoEngine.Projects;

using System;
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
    void LoadCastLibs(ILingoCastLibsContainer castlibContainer, LingoPlayer lingoPlayer);
    ILingoMovie? LoadStartupMovie(ILingoServiceProvider serviceProvider, LingoPlayer lingoPlayer);
    void Run(ILingoMovie movie, bool autoPlayMovie);
}
