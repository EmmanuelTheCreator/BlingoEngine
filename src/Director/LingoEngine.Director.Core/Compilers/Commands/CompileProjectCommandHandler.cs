using AbstUI.Commands;

namespace LingoEngine.Director.Core.Compilers.Commands;

public class CompileProjectCommandHandler : IAbstCommandHandler<CompileProjectCommand>
{
    private readonly LingoScriptCompiler _compiler;

    public CompileProjectCommandHandler(LingoScriptCompiler compiler)
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
