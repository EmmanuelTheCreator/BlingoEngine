using Godot;
using System.Collections.Generic;
using LingoEngine.Movies;
using LingoEngine.Director.Core.Scores;
using LingoEngine.Director.LGodot.Scores.Transitions;
using LingoEngine.Transitions;
using LingoEngine.Director.Core.Tools;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Commands;

namespace LingoEngine.Director.LGodot.Scores;

internal partial class DirGodotTransitionGridChannel : DirGodotTopGridChannel<ILingoSpriteTransitionManager, DirGodotTransitionSprite, LingoTransitionSprite>
{
    public DirGodotTransitionGridChannel(DirScoreGfxValues gfxValues, IDirectorEventMediator mediator, ILingoFrameworkFactory factory, ILingoCommandManager commandManager) 
        : base(gfxValues, mediator, factory, commandManager)
    {
    }

    protected override DirGodotTransitionSprite CreateUISprite(LingoTransitionSprite sprite) => new DirGodotTransitionSprite();

    protected override ILingoSpriteTransitionManager GetManager(LingoMovie movie) => movie.Transitions;
}
