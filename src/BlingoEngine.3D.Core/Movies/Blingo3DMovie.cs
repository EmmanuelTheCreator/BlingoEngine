using BlingoEngine.Casts;
using BlingoEngine.Core;
using BlingoEngine.Events;
using BlingoEngine.Members;
using BlingoEngine.Movies;
using BlingoEngine.Projects;
using BlingoEngine.Stages;
using BlingoEngine.Transitions;

namespace BlingoEngine.L3D.Core.Movies;

/// <summary>
/// Movie subclass exposing basic 3D related properties.
/// </summary>
public class Blingo3DMovie : BlingoMovie
{
   

    public string Active3dRenderer { get; set; } = string.Empty;
    public string Preferred3dRenderer { get; set; } = string.Empty;

    protected internal Blingo3DMovie(BlingoMovieEnvironment environment, BlingoStage movieStage, IBlingoTransitionPlayer transitionPlayer, BlingoCastLibsContainer castLibContainer, IBlingoMemberFactory memberFactory, string name, int number, BlingoEventMediator mediator, Action<BlingoMovie> onRemoveMe, BlingoProjectSettings projectSettings, IBlingoFrameLabelManager blingoFrameLabelManager) : base(environment, movieStage, transitionPlayer, castLibContainer, memberFactory, name, number, mediator, onRemoveMe, projectSettings, blingoFrameLabelManager)
    {
    }
}

