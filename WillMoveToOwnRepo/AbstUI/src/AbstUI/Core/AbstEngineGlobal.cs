namespace AbstUI.Core
{
    public enum AbstUIEngineRunFramework
    {
        None = 0,
        Godot = 1,
        Unity = 2,
        MonoGame = 3,
        Custom = 4,
        SDL2 = 5
    }
    public class AbstUIEngineGlobal
    {
        public static AbstUIEngineRunFramework RunFramework { get; set; } = AbstUIEngineRunFramework.None;
        public static bool IsRunningDirector { get; set; }
    }
}
