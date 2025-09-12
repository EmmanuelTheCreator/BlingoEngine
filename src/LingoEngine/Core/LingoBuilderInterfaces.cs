using LingoEngine.Casts;
using LingoEngine.Movies;

namespace LingoEngine.Core;

public interface ILingoCastLibBuilder
{
    void Build(ILingoCastLibsContainer castLibs);
}

public interface ILingoScoreBuilder
{
    void Build(ILingoMovie movie);
}

public interface ILingoMovieBuilder
{
    ILingoMovie Build(ILingoPlayer player);
}

