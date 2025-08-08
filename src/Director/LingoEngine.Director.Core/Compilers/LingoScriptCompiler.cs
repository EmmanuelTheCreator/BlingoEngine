using System.CodeDom.Compiler;
using System.IO;
using Microsoft.CSharp;
using System.Runtime.Loader;
using System.Reflection;
using System.Linq;
using LingoEngine.Movies;
using LingoEngine.Projects;
using LingoEngine.Director.Core.Projects;
using LingoEngine.Setup;
using Microsoft.Extensions.Logging;

namespace LingoEngine.Director.Core.Compilers;

/// <summary>
/// Compiles project scripts and reloads the active movie.
/// </summary>
public class LingoScriptCompiler
{
    private readonly LingoProjectSettings _settings;
    private readonly DirectorProjectSettings _dirSettings;
    private readonly DirectorProjectManager _projectManager;
    private readonly LingoEngineRegistration _engineRegistration;
    private readonly ILogger<LingoScriptCompiler> _logger;
    private LingoPlayer _player;
    private AssemblyLoadContext? _loadContext;
    public LingoScriptCompiler(
        LingoPlayer player,
        LingoProjectSettings settings,
        DirectorProjectSettings dirSettings,
        DirectorProjectManager projectManager,
        ILingoEngineRegistration engineRegistration,
        ILogger<LingoScriptCompiler> logger)
    {
        _player = player;
        _settings = settings;
        _dirSettings = dirSettings;
        _projectManager = projectManager;
        _engineRegistration = (LingoEngineRegistration)engineRegistration;
        _logger = logger;
    }

    /// <summary>
    /// Builds the configured C# project and reloads the movie.
    /// </summary>
    public void Compile()
    {
        var active = _player.ActiveMovie as LingoMovie;
        var wasPlaying = active?.IsPlaying == true;
        if (active != null)
        {
            active.Halt();
            _player.CloseMovie(active);
        }

        RemoveRegistrations();
        UnloadAssembly();
        var assembly = BuildProject();
        if (assembly != null)
        {
            RegisterFactory(assembly);
            _projectManager.LoadMovie();
        }

        if (wasPlaying && _player.ActiveMovie is LingoMovie movie)
            movie.Play();
    }

    private Assembly? BuildProject()
    {
        if (string.IsNullOrWhiteSpace(_dirSettings.CsProjFile))
        {
            _logger.LogWarning("No C# project file configured");
            return null;
        }

        var projPath = Path.Combine(_settings.ProjectFolder, _dirSettings.CsProjFile);
        var outputPath = Path.ChangeExtension(projPath, "dll");
        var sourceDir = Path.GetDirectoryName(projPath)!;
        var sources = Directory.GetFiles(sourceDir, "*.cs", SearchOption.AllDirectories);

        var csc = new CSharpCodeProvider();
        var parameters = new CompilerParameters
        {
            GenerateExecutable = false,
            OutputAssembly = outputPath
        };
        parameters.ReferencedAssemblies.Add(typeof(LingoPlayer).Assembly.Location);

        var results = csc.CompileAssemblyFromFile(parameters, sources);
        if (results.Errors.HasErrors)
        {
            foreach (CompilerError error in results.Errors)
                _logger.LogError(error.ErrorText);
            return null;
        }

        _logger.LogInformation("Compilation succeeded");

        _loadContext = new AssemblyLoadContext("LingoScripts", true);
        return _loadContext.LoadFromAssemblyPath(outputPath);
    }

    private void RegisterFactory(Assembly assembly)
    {
        var factoryType = assembly.GetTypes().FirstOrDefault(t => typeof(ILingoProjectFactory).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);
        if (factoryType == null)
            return;

        var factory = (ILingoProjectFactory?)Activator.CreateInstance(factoryType);
        if (factory == null)
            return;

        _engineRegistration.ServicesLingo(factory.Setup);
        _player = _engineRegistration.Build();
        factory.Run(_engineRegistration.ServiceProvider!);
    }

    private void RemoveRegistrations()
    {
        _engineRegistration.ClearDynamicRegistrations();
    }

    private void UnloadAssembly()
    {
        if (_loadContext == null)
            return;
        _loadContext.Unload();
        _loadContext = null;
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
}
