using AbstUI.Inputs;
using AbstUI.LGodot.Inputs;
using Godot;
using BlingoEngine.Inputs;

namespace BlingoEngine.LGodot;

public partial class BlingoGodotKey : AbstGodotKey, IBlingoFrameworkKey
{
    public BlingoGodotKey(Node root, Lazy<AbstKey> key) : base(root, key)
    {
    }
}

