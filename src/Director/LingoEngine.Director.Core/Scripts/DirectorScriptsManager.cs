using AbstUI.Commands;
using LingoEngine.Director.Core.FileSystems;
using LingoEngine.Director.Core.Scripts.Commands;
using LingoEngine.Projects;
using LingoEngine.Scripts;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngine.Director.Core.Scripts
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

        private void OpenScript(LingoMemberScript script, int lineNumber)
        {
            var settings = _serviceProvider.GetRequiredService<LingoProjectSettings>();
            var theRootFolder = !string.IsNullOrWhiteSpace(settings.CodeFolder) ? settings.CodeFolder : settings.ProjectFolder;
            var fileName = Path.Combine(theRootFolder, script.FileName);
            if (File.Exists(fileName))
                _ideLauncher.OpenFile(settings, fileName, lineNumber);
        }
    }
}
