using System;
using LingoEngine.Projects;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngine.Demo.TetriGrounds.Core;

public class TetriGroundsProjectFactory : ILingoProjectFactory
{
    public void Setup(IServiceCollection services)
    {
        services.AddTetriGrounds(_ => { });
    }

    public void Run(IServiceProvider serviceProvider)
    {
        var game = serviceProvider.GetRequiredService<TetriGroundsGame>();
        game.LoadMovie();
        game.Play();
    }
}
