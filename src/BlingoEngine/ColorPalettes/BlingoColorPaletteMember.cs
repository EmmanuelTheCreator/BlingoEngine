using AbstUI.Primitives;
using BlingoEngine.Casts;
using BlingoEngine.Members;

namespace BlingoEngine.ColorPalettes
{
    public class BlingoColorPaletteMember : BlingoMember
    {
        private int _colorPaletteId;
        private int _rate;
        private int _cycles = 10;
        private BlingoColorPaletteAction _action = BlingoColorPaletteAction.PaletteTransition;
        private BlingoColorPaletteTransitionOption _transitionOption = BlingoColorPaletteTransitionOption.DontFade;
        private BlingoColorPaletteCycleOption _cycleOption = BlingoColorPaletteCycleOption.AutoReverse;

        public int ColorPaletteId
        {
            get => _colorPaletteId;
            set => SetProperty(ref _colorPaletteId, value);
        }
        /// <summary>
        /// Range between 1 to 30 FPS
        /// in FPS
        /// </summary>
        public int Rate
        {
            get => _rate;
            set => SetProperty(ref _rate, value);
        }
        /// <summary>
        /// When Action is Cycling, the number of cycles.
        /// </summary>
        public int Cycles
        {
            get => _cycles;
            set => SetProperty(ref _cycles, value);
        }
        public BlingoColorPaletteAction Action
        {
            get => _action;
            set => SetProperty(ref _action, value);
        }
        public BlingoColorPaletteTransitionOption TransitionOption
        {
            get => _transitionOption;
            set => SetProperty(ref _transitionOption, value);
        }
        public BlingoColorPaletteCycleOption CycleOption
        {
            get => _cycleOption;
            set => SetProperty(ref _cycleOption, value);
        }
        public BlingoColorPaletteMember(BlingoCast cast, int numberInCast, string name = "", string fileName = "", APoint regPoint = default) : base(new BlingoColorPaletteMemberFW(), BlingoMemberType.Palette, cast, numberInCast, name, fileName, regPoint)
        {
        }

        public void SetSettings(BlingoColorPaletteFrameSettings settings)
        {
            ColorPaletteId = settings.ColorPaletteId;
            Rate = settings.Rate;
            Action = settings.Action;
            CycleOption = settings.CycleOption;
            TransitionOption = settings.TransitionOption;
        }

        public BlingoColorPaletteFrameSettings GetSettings()
        {
            return new BlingoColorPaletteFrameSettings
            {
                Action = Action,
                ColorPaletteId = ColorPaletteId,
                CycleOption = CycleOption,
                Cycles = Cycles,
                Rate = Rate,
                TransitionOption = TransitionOption,
            };
        }
    }
}

