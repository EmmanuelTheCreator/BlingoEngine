using LingoEngine.Core;

namespace LingoEngine.VerboseLanguage
{
    /*
  OMG ! :-)
 -----------------------------------------------------------
 on ToggleActions whichSprite
     sprite (whichSprite).actionsEnabled = not sprite
     (whichSprite).actionsEnabled
 end

 Verbose syntax:
 on ToggleActions whichSprite
     set the actionsEnabled of sprite whichSprite = not the actionsEnabled of
     sprite whichSprite
 end
 -----------------------------------------------------------
 set characterAlign = the alignment of member "Rokujo Speaks"
 -----------------------------------------------------------
 */

    public static class LingoVerboseExtensions
    {
        #region Player
        public static T Get<T>(this ILingoPlayer player, Func<ILingoVerboseThe, ILingoVerbosePropAccess<T>> actionOnThe)
        {
            var propAccess = actionOnThe(new LingoVerboseThe((LingoPlayer)player));
            return propAccess.Value;
        }
        public static void Set<T>(this ILingoPlayer player, Func<ILingoVerboseThe, ILingoVerbosePropAccess<T>> actionOnThe, T value)
        {
            var propAccess = actionOnThe(new LingoVerboseThe((LingoPlayer)player));
            propAccess.Value = value;
        }
        public static ILingoVerbosePropAccess<T> Set<T>(this ILingoPlayer player, Func<ILingoVerboseThe, ILingoVerbosePropAccess<T>> actionOnThe)
        {
            var propAccess = actionOnThe(new LingoVerboseThe((LingoPlayer)player));
            return propAccess;
        }

        public static ILingoVerbosePut Put(this ILingoPlayer player, object? value) => new LingoVerbosePut((LingoPlayer)player, value);
        public static ILingoVerboseThe The(this ILingoPlayer player) => new LingoVerboseThe((LingoPlayer)player);

        #endregion





        #region LingoScriptBase
        public static T Get<T>(this ILingoScriptBase scriptBase, Func<ILingoVerboseThe, ILingoVerbosePropAccess<T>> actionOnThe)
          => scriptBase.Player.Get(actionOnThe);
        public static void Set<T>(this ILingoScriptBase scriptBase, Func<ILingoVerboseThe, ILingoVerbosePropAccess<T>> actionOnThe, T value)
            => scriptBase.Player.Set(actionOnThe, value);
        public static ILingoVerbosePropAccess<T> Set<T>(this ILingoScriptBase scriptBase, Func<ILingoVerboseThe, ILingoVerbosePropAccess<T>> actionOnThe)
            => scriptBase.Player.Set(actionOnThe);
        public static ILingoVerbosePut Put(this ILingoScriptBase scriptBase, object? value)
            => scriptBase.Player.Put(value);

        #endregion
    }

}
