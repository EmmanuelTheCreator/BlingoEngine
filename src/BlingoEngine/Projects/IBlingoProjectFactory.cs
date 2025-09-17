namespace BlingoEngine.Projects;

using System;
using System.Threading.Tasks;
using BlingoEngine.Core;
using BlingoEngine.Setup;
using BlingoEngine.Casts;
using BlingoEngine.Movies;

/// <summary>
/// Lingo Project Factory interface.
/// </summary>
public interface IBlingoProjectFactory
{
    void Setup(IBlingoEngineRegistration engineRegistration);
    Task LoadCastLibsAsync(IBlingoCastLibsContainer castlibContainer, BlingoPlayer blingoPlayer);
    Task<IBlingoMovie?> LoadStartupMovieAsync(IBlingoServiceProvider serviceProvider, BlingoPlayer blingoPlayer);
    void Run(IBlingoMovie movie, bool autoPlayMovie);
}

