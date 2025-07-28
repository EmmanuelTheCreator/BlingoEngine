using Godot;
using LingoEngine.Movies;

namespace LingoEngine.Director.LGodot.Scores;

internal interface IDirMovieNode
{
    void SetMovie(LingoMovie? movie);
}

internal interface IDirScrollX
{
    float ScrollX { get; set; }
}

internal interface IDirCollapsibleHeader
{
    bool Collapsed { get; set; }
}
