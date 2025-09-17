namespace BlingoEngine.Sounds
{
    /// <summary>
    /// Lingo Framework Sound interface.
    /// </summary>
    public interface IBlingoFrameworkSound
    {
        List<BlingoSoundDevice> SoundDeviceList { get; }
        int SoundLevel { get; set; }
        bool SoundEnable { get; set; }
        void Beep();
    }
}

