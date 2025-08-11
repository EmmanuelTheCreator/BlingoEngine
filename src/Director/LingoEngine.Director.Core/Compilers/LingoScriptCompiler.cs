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
using LingoEngine.Core;
using System.Diagnostics;
using System.Text.RegularExpressions;

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
        var wasPlaying = (_player.ActiveMovie as LingoMovie)?.IsPlaying == true;
        _engineRegistration.UnloadMovie("Director");
        UnloadAssembly();
        var assembly = BuildProject();
        if (assembly != null)
        {
            RegisterFactory(assembly);
            //_projectManager.LoadMovie();
        }

        if (wasPlaying && _player.ActiveMovie is LingoMovie movie)
            movie.Play();
    }
    private Assembly? BuildProject()
    {
        if (string.IsNullOrWhiteSpace(_dirSettings.CsProjFile))
            return null;

        var projPath = Path.Combine(_settings.ProjectFolder, _dirSettings.CsProjFile);
        //var outputPath = Path.Combine(_settings.ProjectFolder, "TempOutput");
        var outputPath = "C:\\Temp\\Director";
        var intermediatePath = Path.Combine(outputPath, "obj");
        if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);
        if (!Directory.Exists(intermediatePath)) Directory.CreateDirectory(intermediatePath);
        var arguments = $"build \"{projPath}\" -c Debug " +
                $"-p:OutputPath=\"{outputPath}\" " +
                $"-p:OutDir=\"{outputPath}\" " +
                $"-p:BaseOutputPath=\"{outputPath}\" " +
                $"-p:IntermediateOutputPath=\"{intermediatePath.Replace("\\","/")}\"/";
        var psi = new ProcessStartInfo("dotnet", arguments)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var process = new Process { StartInfo = psi };

        bool buildFailed = false;

        process.OutputDataReceived += (s, e) =>
        {
            if (e.Data == null) return;
            _logger.LogInformation(e.Data);

            if (e.Data.Contains(" error ", StringComparison.OrdinalIgnoreCase) ||
                Regex.IsMatch(e.Data, @"\berror\s+CS\d{4}\b"))
                buildFailed = true;
        };

        process.ErrorDataReceived += (s, e) =>
        {
            if (e.Data == null) return;
            _logger.LogError(e.Data);

            if (e.Data.Contains("error", StringComparison.OrdinalIgnoreCase))
                buildFailed = true;
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        if (buildFailed)
        {
            _logger.LogError("Build failed.");
            return null;
        }


        var dllName = Path.GetFileNameWithoutExtension(projPath) + ".dll";
        var dllPath = Path.Combine(outputPath, dllName);

        if (!File.Exists(dllPath))
        {
            _logger.LogError($"Expected output DLL not found: {dllPath}");
            return null;
        }

        _loadContext = new AssemblyLoadContext("LingoScripts", isCollectible: true);
        return _loadContext.LoadFromAssemblyPath(dllPath);
    }
    private bool BuildFailed(string stdout, string stderr)
    {
        // Matches: "1 Error(s)", "15 Errors", "error CS1001", etc.
        var hasErrorCount = Regex.IsMatch(stdout, @"\b\d+\s+Error\(s\)", RegexOptions.IgnoreCase);
        var hasCsError = Regex.IsMatch(stdout, @"\berror\s+CS\d{4}\b", RegexOptions.IgnoreCase);
        var fatal = Regex.IsMatch(stderr, @"\bfatal error\b", RegexOptions.IgnoreCase);

        return hasErrorCount || hasCsError || fatal;
    }
    private Assembly? BuildProject2()
    {
        try
        {

       
            if (string.IsNullOrWhiteSpace(_dirSettings.CsProjFile))
            {
                _logger.LogWarning("No C# project file configured");
                return null;
            }

            var projPath = Path.Combine(_settings.ProjectFolder, _dirSettings.CsProjFile);
            var outputPath = Path.Combine(_settings.ProjectFolder, _dirSettings.CsProjFile.Replace(".csproj", "",StringComparison.InvariantCultureIgnoreCase), "bin", "debug");
            var sourceDir = Path.GetDirectoryName(projPath)!;
            var sources = Directory.GetFiles(sourceDir, "*.cs", SearchOption.AllDirectories).Where(path => !path.Contains(@"\bin\") && !path.Contains(@"\obj\")).ToArray();
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);
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
        catch (Exception e)
        {
            _logger.LogError(e, "Error while compiling:" + e.Message);
            return null;
        }
    }

    private void RegisterFactory(Assembly assembly)
    {
        var factoryType = assembly.GetTypes().FirstOrDefault(t => typeof(ILingoProjectFactory).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);
        if (factoryType == null)
            return;

        var factory = (ILingoProjectFactory?)Activator.CreateInstance(factoryType);
        if (factory == null)
            return;
        _engineRegistration.SetTheProjectFactory(factoryType);
        _engineRegistration.BuildAndRunProject();
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
