namespace LingoEngine.Projects;

using System;
using LingoEngine.Core;
using LingoEngine.Setup;
using LingoEngine.Casts;

public interface ILingoProjectFactory
{
    void Setup(ILingoEngineRegistration engineRegistration);
    void Run(IServiceProvider serviceProvider, ILingoPlayer player, bool autoPlayMovie);
    void LoadCastLibs(ILingoCastLibsContainer castlibContainer, LingoPlayer lingoPlayer);
}
