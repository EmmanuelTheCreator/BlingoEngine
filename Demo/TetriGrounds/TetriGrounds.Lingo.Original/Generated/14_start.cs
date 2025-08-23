public class startBehavior : LingoSpriteBehavior, IHasExitFrameEvent
{
    public startBehavior(ILingoMovieEnvironment env) : base(env) { }
public void ExitFrame()
{
    _Movie.GoTo("Game");
}

}
