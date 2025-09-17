using AbstUI.Commands;
using BlingoEngine.Director.Core.FileSystems;
using BlingoEngine.Director.Core.Scripts.Commands;
using BlingoEngine.Projects;
using BlingoEngine.Scripts;
using Microsoft.Extensions.DependencyInjection;

namespace BlingoEngine.Director.Core.Scripts
{
    internal interface IDirectorScriptsManager 
    {
    }

    public class DirectorScriptsManager : IDirectorScriptsManager, IAbstCommandHandler<OpenScriptCommand>
    {
        private readonly IIdeLauncher _ideLauncher;
        private readonly IServiceProvider _serviceProvider;

        public DirectorScriptsManager(IIdeLauncher ideLauncher, IServiceProvider serviceProvider)
        {
            _ideLauncher = ideLauncher;
            _serviceProvider = serviceProvider;
        }

        public bool CanExecute(OpenScriptCommand command) => true;
        public bool Handle(OpenScriptCommand command)
        {
            OpenScript(command.Script, command.LineNumber); 
            return true;
        }

        private void OpenScript(BlingoMemberScript script, int lineNumber)
        {
            var settings = _serviceProvider.GetRequiredService<BlingoProjectSettings>();
            var theRootFolder = !string.IsNullOrWhiteSpace(settings.CodeFolder) ? settings.CodeFolder : settings.ProjectFolder;
            var fileName = Path.Combine(theRootFolder, script.FileName);
            if (File.Exists(fileName))
                _ideLauncher.OpenFile(settings, fileName, lineNumber);
        }
    }
}

