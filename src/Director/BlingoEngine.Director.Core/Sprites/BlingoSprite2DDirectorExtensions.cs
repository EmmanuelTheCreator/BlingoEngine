using AbstUI.Commands;
using BlingoEngine.Director.Core.Inspector.Commands;
using BlingoEngine.Scripts;
using BlingoEngine.Sprites;

namespace BlingoEngine.Director.Core.Sprites;

public static class BlingoSprite2DDirectorExtensions
{
    public static void AttachBehavior(this BlingoSprite2D sprite, BlingoMemberScript script, IAbstCommandManager commandManager)
    {
        
        BlingoSpriteBehavior? behavior = sprite.AddBehavior(script);
        if (behavior == null) return;
        if (behavior is IBlingoPropertyDescriptionList)
            commandManager.Handle(new OpenBehaviorPopupCommand(behavior));
        //sprite.SpriteChannel?.RequireRedraw();
    }

}

