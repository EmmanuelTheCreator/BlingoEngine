using AbstUI.Primitives;
using LingoEngine.Casts;
using LingoEngine.Members;

namespace LingoEngine.ColorPalettes
{
    public class LingoColorPaletteMember : LingoMember
    {
        private int _colorPaletteId;
        private int _rate;
        private int _cycles = 10;
        private LingoColorPaletteAction _action = LingoColorPaletteAction.PaletteTransition;
        private LingoColorPaletteTransitionOption _transitionOption = LingoColorPaletteTransitionOption.DontFade;
        private LingoColorPaletteCycleOption _cycleOption = LingoColorPaletteCycleOption.AutoReverse;

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
        public LingoColorPaletteAction Action
        {
            get => _action;
            set => SetProperty(ref _action, value);
        }
        public LingoColorPaletteTransitionOption TransitionOption
        {
            get => _transitionOption;
            set => SetProperty(ref _transitionOption, value);
        }
        public LingoColorPaletteCycleOption CycleOption
        {
            get => _cycleOption;
            set => SetProperty(ref _cycleOption, value);
        }
        public LingoColorPaletteMember(LingoCast cast, int numberInCast, string name = "", string fileName = "", APoint regPoint = default) : base(new LingoColorPaletteMemberFW(), LingoMemberType.Palette, cast, numberInCast, name, fileName, regPoint)
        {
        }

        public void SetSettings(LingoColorPaletteFrameSettings settings)
        {
            ColorPaletteId = settings.ColorPaletteId;
            Rate = settings.Rate;
            Action = settings.Action;
            CycleOption = settings.CycleOption;
            TransitionOption = settings.TransitionOption;
        }

        public LingoColorPaletteFrameSettings GetSettings()
        {
            return new LingoColorPaletteFrameSettings
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
