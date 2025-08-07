namespace LingoEngine.Core
{
    public enum LingoEngineRunFramework
    {
        None = 0,
        Godot = 1,
        Unity = 2,
        MonoGame = 3,
        Custom = 4,
        SDL2 = 5
    }
    public class LingoEngineGlobal
    {
        public static LingoEngineRunFramework RunFramework { get; set; } = LingoEngineRunFramework.None;
    }
}
