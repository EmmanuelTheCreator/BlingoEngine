using LingoEngine.Commands;
using LingoEngine.Director.Core.Compilers;

namespace LingoEngine.Director.Core.Compilers.Commands;

public class CompileProjectCommandHandler : ICommandHandler<CompileProjectCommand>
{
    private readonly LingoScriptCompiler _compiler;

    public CompileProjectCommandHandler(LingoScriptCompiler compiler)
    {
        _compiler = compiler;
    }

    public bool Handle(CompileProjectCommand command)
    {
        _compiler.Compile();
        return true;
    }
}
