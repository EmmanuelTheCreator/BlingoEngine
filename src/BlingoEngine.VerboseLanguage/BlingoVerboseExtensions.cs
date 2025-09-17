using BlingoEngine.Core;

namespace BlingoEngine.VerboseLanguage
{
    /*
  OMG ! :-)
 -----------------------------------------------------------
 on ToggleActions whichSprite
     sprite (whichSprite).actionsEnabled = not sprite (whichSprite).actionsEnabled
 end

 Verbose syntax:
 on ToggleActions whichSprite
     set the actionsEnabled of sprite whichSprite = not the actionsEnabled of sprite whichSprite
 end
 -----------------------------------------------------------
 set characterAlign = the alignment of member "Rokujo Speaks"
 -----------------------------------------------------------
 */

    public static class BlingoVerboseExtensions
    {
        #region Player
        public static T Get<T>(this IBlingoPlayer player,IBlingoVerbosePropAccess<T> thePropAccess) => thePropAccess.Value;
        //public static T Get<T>(this IBlingoPlayer player, Func<IBlingoVerboseThe, IBlingoVerbosePropAccess<T>> actionOnThe)
        //{
        //    var propAccess = actionOnThe(new BlingoVerboseThe((BlingoPlayer)player));
        //    return propAccess.Value;
        //}
        public static IBlingoVerbosePropAccess<T> Set<T>(this IBlingoPlayer player, IBlingoVerbosePropAccess<T> thePropAccess) => thePropAccess;
        //public static void Set<T>(this IBlingoPlayer player, Func<IBlingoVerboseThe, IBlingoVerbosePropAccess<T>> actionOnThe, T value)
        //{
        //    var propAccess = actionOnThe(new BlingoVerboseThe((BlingoPlayer)player));
        //    propAccess.Value = value;
        //}
        //public static IBlingoVerbosePropAccess<T> Set<T>(this IBlingoPlayer player, Func<IBlingoVerboseThe, IBlingoVerbosePropAccess<T>> actionOnThe)
        //{
        //    var propAccess = actionOnThe(new BlingoVerboseThe((BlingoPlayer)player));
        //    return propAccess;
        //}

        public static IBlingoVerbosePut Put(this IBlingoPlayer player, object? value) => new BlingoVerbosePut((BlingoPlayer)player, value);
        public static IBlingoVerboseThe The(this IBlingoPlayer player) => new BlingoVerboseThe((BlingoPlayer)player);
        public static bool Not(this IBlingoPlayer player, bool state) => !state;
        public static bool Not(this IBlingoVerbosePropAccess<bool> currentValue) => !currentValue.Value;
        public static bool Not(this IBlingoPlayer player, IBlingoVerbosePropAccess<bool> currentValue) => !currentValue.Value;

        #endregion





        #region BlingoScriptBase
        public static T Get<T>(this IBlingoScriptBase scriptBase, IBlingoVerbosePropAccess<T> thePropAccess) => scriptBase.Player.Get(thePropAccess);
        //public static T Get<T>(this IBlingoScriptBase scriptBase, Func<IBlingoVerboseThe, IBlingoVerbosePropAccess<T>> actionOnThe) => scriptBase.Player.Get(actionOnThe);

        public static IBlingoVerbosePropAccess<T> Set<T>(this IBlingoScriptBase scriptBase, IBlingoVerbosePropAccess<T> thePropAccess) => scriptBase.Player.Set(thePropAccess);
        //public static void Set<T>(this IBlingoScriptBase scriptBase, Func<IBlingoVerboseThe, IBlingoVerbosePropAccess<T>> actionOnThe, T value) => scriptBase.Player.Set(actionOnThe, value);
        //public static IBlingoVerbosePropAccess<T> Set<T>(this IBlingoScriptBase scriptBase, Func<IBlingoVerboseThe, IBlingoVerbosePropAccess<T>> actionOnThe) => scriptBase.Player.Set(actionOnThe);

        public static IBlingoVerbosePut Put(this IBlingoScriptBase scriptBase, object? value) => scriptBase.Player.Put(value);
        public static IBlingoVerboseThe The(this IBlingoScriptBase scriptBase) => scriptBase.Player.The();

        public static bool Not(this IBlingoScriptBase scriptBase, bool state) => scriptBase.Player.Not(state);

        #endregion
    }

}

