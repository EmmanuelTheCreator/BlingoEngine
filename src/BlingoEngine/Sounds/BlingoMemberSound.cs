using BlingoEngine.Casts;
using BlingoEngine.Members;

namespace BlingoEngine.Sounds
{
    public class BlingoMemberSound : BlingoMember
    {
        private readonly IBlingoFrameworkMemberSound _blingoFrameworkMemberSound;
        public T Framework<T>() where T : IBlingoFrameworkMemberSound => (T)_blingoFrameworkMemberSound;

        /// <summary>
        /// Whether this sound member is stereo (true) or mono (false). Default is mono.
        /// Lingo: the stereo of member
        /// </summary>
        public bool Stereo => _blingoFrameworkMemberSound.Stereo;
        /// <summary>
        ///  length of this audio stream, in seconds.
        /// </summary>
        public double Length => _blingoFrameworkMemberSound.Length;

        /// <summary>
        /// Indicates whether this sound loops by default.
        /// Lingo: the loop of member
        /// </summary>
        private bool _loop;
        public bool Loop
        {
            get => _loop;
            set => SetProperty(ref _loop, value);
        }

        /// <summary>
        /// Whether this member is externally linked (not embedded).
        /// Lingo: the linked of member
        /// </summary>
        public bool IsLinked { get; set; } = false;

        /// <summary>
        /// The path of the external file, if linked. Optional.
        /// </summary>
        public string LinkedFilePath { get; set; } = string.Empty;

        public BlingoMemberSound(IBlingoFrameworkMemberSound blingoMemberSound, BlingoCast cast, int numberInCast, string name = "", string fileName = "")
            : base(blingoMemberSound, BlingoMemberType.Sound, cast, numberInCast, name, fileName)
        {
            _blingoFrameworkMemberSound = blingoMemberSound;

        }
        protected override BlingoMember OnDuplicate(int newNumber)
        {
            throw new NotImplementedException("_blingoFrameworkMemberSound has to be retieved from the factory");
            //var clone = new BlingoMemberSound(_blingoFrameworkMemberSound, _cast, newNumber, Name);
            //clone.Loop = Loop;
            //clone.IsLinked = IsLinked;
            //clone.LinkedFilePath = LinkedFilePath;
            //return clone;
        }

        public bool IsExternal => IsLinked && !string.IsNullOrWhiteSpace(LinkedFilePath);
    }


}

