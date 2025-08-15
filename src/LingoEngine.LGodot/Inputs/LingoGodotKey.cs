using AbstUI.Inputs;
using Godot;
using LingoEngine.Inputs;

namespace LingoEngine.LGodot;

public partial class LingoGodotKey : AbstGodotKey, ILingoFrameworkKey
{
    public LingoGodotKey(Node root, Lazy<AbstKey> key) : base(root, key)
    {
    }
}
