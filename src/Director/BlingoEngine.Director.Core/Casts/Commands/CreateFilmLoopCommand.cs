using System.Collections.Generic;
using AbstUI.Commands;
using BlingoEngine.Movies;
using BlingoEngine.Sprites;

namespace BlingoEngine.Director.Core.Casts.Commands;

/// <summary>
/// Command for creating a film loop member from a set of sprites.
/// </summary>
/// <param name="Movie">Target movie whose active cast will receive the new member.</param>
/// <param name="Sprites">Sprites to include in the film loop.</param>
/// <param name="Name">Name of the new film loop member.</param>
public sealed record CreateFilmLoopCommand(
    BlingoMovie Movie,
    IEnumerable<BlingoSprite> Sprites,
    string Name) : IAbstCommand;


