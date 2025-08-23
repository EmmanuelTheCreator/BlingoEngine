public class Stop_MenuBehavior : LingoSpriteBehavior, IHasExitFrameEvent
{
    public Stop_MenuBehavior(ILingoMovieEnvironment env) : base(env) { }
public void ExitFrame()
{
    _Movie.GoTo(_Movie.CurrentFrame);
}

}
