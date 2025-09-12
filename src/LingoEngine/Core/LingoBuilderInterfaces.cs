using LingoEngine.Casts;
using LingoEngine.Movies;

namespace LingoEngine.Core;

public interface ILingoCastLibBuilder
{
    Task BuildAsync(ILingoCastLibsContainer castLibs);
}

public interface ILingoScoreBuilder
{
    Task BuildAsync(ILingoMovie movie);
}

public interface ILingoMovieBuilder
{
    Task<ILingoMovie> BuildAsync(ILingoPlayer player);
}

