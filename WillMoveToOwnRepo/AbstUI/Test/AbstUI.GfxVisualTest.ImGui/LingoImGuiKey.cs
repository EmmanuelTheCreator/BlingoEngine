using AbstUI.Inputs;
using AbstUI.ImGui.Inputs;
using LingoEngine.Inputs;

namespace LingoEngine.ImGui.Inputs;

/// <summary>
/// ImGui implementation of <see cref="ILingoFrameworkKey"/>.
/// </summary>
public class LingoImGuiKey : ImGuiKey, ILingoFrameworkKey
{
    private AbstKey? _key;

    /// <summary>Associates the key with its abstaction wrapper.</summary>
    public void SetKeyObj(AbstKey key) => _key = key;
}

