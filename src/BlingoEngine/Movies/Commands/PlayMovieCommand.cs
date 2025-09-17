using AbstUI.Commands;

namespace BlingoEngine.Movies.Commands;

public sealed record PlayMovieCommand(int? Frame = null) : IAbstCommand;

