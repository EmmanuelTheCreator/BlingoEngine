namespace AbstUI.Core
{
    public enum AbstEngineRunFramework
    {
        None = 0,
        Godot = 1,
        Unity = 2,
        MonoGame = 3,
        Custom = 4,
        SDL2 = 5,
        Blazor = 6
    }
    public class AbstEngineGlobal
    {
        public static AbstEngineRunFramework RunFramework { get; set; } = AbstEngineRunFramework.None;
        public static bool IsRunningDirector { get; set; }
    }
}
