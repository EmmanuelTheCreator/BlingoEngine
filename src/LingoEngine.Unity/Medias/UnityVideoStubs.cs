namespace UnityEngine
{
    /// <summary>
    /// Minimal stub of Unity's VideoClip used for compilation in environments
    /// where the real Video module is unavailable.
    /// </summary>
    public class VideoClip
    {
        public double length { get; set; }
    }

    /// <summary>
    /// Minimal stub of Unity's VideoPlayer component. Provides just enough
    /// functionality for the engine to compile and manage simple playback state.
    /// </summary>
    public class VideoPlayer : Behaviour
    {
        public string url = string.Empty;
        public bool isPlaying { get; private set; }
        public double time { get; set; }
        public VideoClip? clip { get; set; }
        public bool playOnAwake;

        public void Play() => isPlaying = true;
        public void Pause() => isPlaying = false;
        public void Stop()
        {
            isPlaying = false;
            time = 0;
        }
    }
}
