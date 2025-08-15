using LingoEngine.Commands;
using LingoEngine.Director.Core.Inspector.Commands;
using LingoEngine.Scripts;
using LingoEngine.Sprites;

namespace LingoEngine.Director.Core.Sprites;

public static class LingoSprite2DDirectorExtensions
{
    public static void AttachBehavior(this LingoSprite2D sprite, LingoMemberScript script, ILingoCommandManager commandManager)
    {
        
        LingoSpriteBehavior? behavior = sprite.AddBehavior(script);
        if (behavior == null) return;
        if (behavior is ILingoPropertyDescriptionList)
            commandManager.Handle(new OpenBehaviorPopupCommand(behavior));
        //sprite.SpriteChannel?.RequireRedraw();
    }

}
