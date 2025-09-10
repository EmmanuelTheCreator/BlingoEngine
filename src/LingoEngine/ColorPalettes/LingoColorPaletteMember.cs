using AbstUI.Primitives;
using LingoEngine.Casts;
using LingoEngine.Members;

namespace LingoEngine.ColorPalettes
{
    public class LingoColorPaletteMember : LingoMember
    {
        public int ColorPaletteId { get; set; }
        /// <summary>
        /// Range between 1 to 30 FPS
        /// in FPS
        /// </summary>
        public int Rate { get; set; }
        /// <summary>
        /// When Action is Cycling, the number of cycles.
        /// </summary>
        public int Cycles { get; set; } = 10;
        public LingoColorPaletteAction Action { get; set; } = LingoColorPaletteAction.PaletteTransition;
        public LingoColorPaletteTransitionOption TransitionOption { get; set; } = LingoColorPaletteTransitionOption.DontFade;
        public LingoColorPaletteCycleOption CycleOption { get; set; } = LingoColorPaletteCycleOption.AutoReverse;
        public LingoColorPaletteMember(LingoCast cast, int numberInCast, string name = "", string fileName = "", APoint regPoint = default) : base(new LingoColorPaletteMemberFW(), LingoMemberType.Palette , cast, numberInCast, name, fileName, regPoint)
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
