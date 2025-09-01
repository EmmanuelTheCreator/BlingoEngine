using AbstUI.Primitives;
using LingoEngine.Casts;
using LingoEngine.Members;

namespace LingoEngine.Transitions
{
    public enum LingoTransitionAffects
    {
        EntireStage,
        ChangingAreaOnly
    }
    public class LingoTransitionMember : LingoMember
    {
        public int TransitionId { get; set; }
        public string TransitionName { get; set; } = "";
        /// <summary>
        /// Duration in Seconds from 0 to 30 seconds
        /// </summary>
        public float Duration { get; set; }
        public float Smoothness { get; set; }
        public LingoTransitionAffects Affects { get; set; } = LingoTransitionAffects.ChangingAreaOnly;



        public LingoTransitionMember(LingoCast cast, int numberInCast, string name = "", string fileName = "", APoint regPoint = default) : base(null, LingoMemberType.Palette, cast, numberInCast, name, fileName, regPoint)
        {
        }

        public void SetSettings(LingoTransitionFrameSettings settings)
        {
            TransitionId = settings.TransitionId;
            Affects = settings.Affects;
            TransitionId = settings.TransitionId;
            Duration = settings.Duration;
            Smoothness = settings.Smoothness;
            TransitionName = settings.TransitionName;
        }

        public LingoTransitionFrameSettings GetSettings()
        {
            return new LingoTransitionFrameSettings
            {
                Affects = Affects,
                TransitionId = TransitionId,
                Duration = Duration,
                Smoothness = Smoothness,
                TransitionName = TransitionName,
            };
        }
    }
}
