using AbstUI.Commands;

namespace BlingoEngine.Director.Core.Compilers.Commands;

public class CompileProjectCommandHandler : IAbstCommandHandler<CompileProjectCommand>
{
    private readonly BlingoScriptCompiler _compiler;

    public CompileProjectCommandHandler(BlingoScriptCompiler compiler)
    {
        _compiler = compiler;
    }

    public bool CanExecute(CompileProjectCommand command) => true;
    public bool Handle(CompileProjectCommand command)
    {
        _compiler.Compile();
        return true;
    }
}

