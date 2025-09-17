using BlingoEngine.Casts;
using BlingoEngine.Movies;

namespace BlingoEngine.Core;

public interface IBlingoCastLibBuilder
{
    Task BuildAsync(IBlingoCastLibsContainer castLibs);
}

public interface IBlingoScoreBuilder
{
    Task BuildAsync(IBlingoMovie movie);
}

public interface IBlingoMovieBuilder
{
    Task<IBlingoMovie> BuildAsync(IBlingoPlayer player);
}


