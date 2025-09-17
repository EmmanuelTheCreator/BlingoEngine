using Godot;
using BlingoEngine.Sounds;

namespace BlingoEngine.LGodot.Sounds
{
    public class BlingoGodotSound : IBlingoFrameworkSound
    {
        private BlingoSound _blingoSound = null!;
        private List<BlingoSoundDevice> _soundDeviceList = new List<BlingoSoundDevice>();

        // Property to get or set the sound devices list
        public List<BlingoSoundDevice> SoundDeviceList => _soundDeviceList;

        // Property to get or set the sound level (volume control)
        public int SoundLevel
        {
            get => (int)(AudioServer.GetBusVolumeDb(0) * 100);  // Assume the main bus (0) is used for sound
            set => AudioServer.SetBusVolumeDb(0, value / 100f); // Set the volume in dB
        }

        // Property to enable or disable sound
        public bool SoundEnable
        {
            get => AudioServer.IsBusMute(0); // Check if the main bus is muted
            set => AudioServer.SetBusMute(0, !value); // Set the mute state based on the value
        }

        // Initialize with the BlingoSound object (for managing channels)
        internal void Init(BlingoSound sound)
        {
            _blingoSound = sound;

        }

        // Beep functionality - just calls the system beep
        public void Beep()
        {
            Console.Beep();
        }

        // Get a specific channel by its number
        public BlingoSoundChannel Channel(int channelNumber)
        {
            return _blingoSound.Channel(channelNumber);
        }

        // Update device list - Can be expanded for custom logic
        public void UpdateDeviceList()
        {
            // Implementation based on your specific requirements, perhaps refreshing the list of sound devices
        }

    }
}

