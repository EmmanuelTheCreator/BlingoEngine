namespace LingoEngine.Medias.Events
{
    /// <summary>Event interface triggered when video playback starts.</summary>
    public interface IHasStartVideoEvent
    {
        void StartVideo();
    }
    /// <summary>Event interface triggered when video playback stops.</summary>
    public interface IHasStopVideoEvent
    {
        void StopVideo();
    }
    /// <summary>Event interface triggered when video playback pauses.</summary>
    public interface IHasPauseVideoEvent
    {
        void PauseVideo();
    }
    /// <summary>Event interface triggered when video playback finishes.</summary>
    public interface IHasEndVideoEvent
    {
        void EndVideo();
    }
}
