namespace LingoEngine.Projects;

using System;
using Microsoft.Extensions.DependencyInjection;

public interface ILingoProjectFactory
{
    void Setup(IServiceCollection services);
    void Run(IServiceProvider serviceProvider);
}
