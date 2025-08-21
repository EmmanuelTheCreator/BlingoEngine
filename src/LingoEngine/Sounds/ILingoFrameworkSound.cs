namespace LingoEngine.Sounds
{
    /// <summary>
    /// Lingo Framework Sound interface.
    /// </summary>
    public interface ILingoFrameworkSound
    {
        List<LingoSoundDevice> SoundDeviceList { get; }
        int SoundLevel { get; set; }
        bool SoundEnable { get; set; }
        void Beep();
    }
}
