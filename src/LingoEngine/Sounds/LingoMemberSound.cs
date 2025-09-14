﻿using LingoEngine.Casts;
using LingoEngine.Members;

namespace LingoEngine.Sounds
{
    public class LingoMemberSound : LingoMember
    {
        private readonly ILingoFrameworkMemberSound _lingoFrameworkMemberSound;
        public T Framework<T>() where T : ILingoFrameworkMemberSound => (T)_lingoFrameworkMemberSound;

        /// <summary>
        /// Whether this sound member is stereo (true) or mono (false). Default is mono.
        /// Lingo: the stereo of member
        /// </summary>
        public bool Stereo => _lingoFrameworkMemberSound.Stereo;
        /// <summary>
        ///  length of this audio stream, in seconds.
        /// </summary>
        public double Length => _lingoFrameworkMemberSound.Length;

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

        public LingoMemberSound(ILingoFrameworkMemberSound lingoMemberSound, LingoCast cast, int numberInCast, string name = "", string fileName = "")
            : base(lingoMemberSound, LingoMemberType.Sound, cast, numberInCast, name, fileName)
        {
            _lingoFrameworkMemberSound = lingoMemberSound;

        }
        protected override LingoMember OnDuplicate(int newNumber)
        {
            throw new NotImplementedException("_lingoFrameworkMemberSound has to be retieved from the factory");
            //var clone = new LingoMemberSound(_lingoFrameworkMemberSound, _cast, newNumber, Name);
            //clone.Loop = Loop;
            //clone.IsLinked = IsLinked;
            //clone.LinkedFilePath = LinkedFilePath;
            //return clone;
        }

        public bool IsExternal => IsLinked && !string.IsNullOrWhiteSpace(LinkedFilePath);
    }


}
