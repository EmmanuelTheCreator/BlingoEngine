namespace BlingoEngine.Primitives
{
    public enum BlingoInkType
    {
        Copy = 0,
        Transparent = 1,
        Reverse = 2,
        Ghost = 3,
        NotCopy = 4,
        NotTransparent = 5,
        NotReverse = 6,
        NotGhost = 7,
        Matte = 8,
        Mask = 9,
        Blend = 32,
        AddPin = 33,
        Add = 34,
        SubstractPin = 35,
        BackgroundTransparent = 36,
        Lightest = 37,
        Substract = 38,
        Darkest = 39,
        Lighten = 40,
        Darken = 41
    }
    [Flags]
    public enum BlingoInkBlendFlags
    {
        None = 0x00,

        /// <summary>Marks this ink as a blend-mode ink (base flag).</summary>
        BlendBase = 0x20,

        /// <summary>Clamp result (e.g. AddPin, SubtractPin).</summary>
        Pin = 0x01,

        /// <summary>Additive blend.</summary>
        Add = 0x02,

        /// <summary>Subtractive blend.</summary>
        Subtract = 0x06,

        /// <summary>Maximum (lightest) color.</summary>
        Max = 0x05,

        /// <summary>Minimum (darkest) color.</summary>
        Min = 0x07,

        /// <summary>Lighten blend logic.</summary>
        Lighten = 0x08,

        /// <summary>Darken blend logic.</summary>
        Darken = 0x09,

        /// <summary>Special case for background-aware mouse hit testing.</summary>
        BackgroundTransparent = 0x04,
    }
    public static class BlingoInkBlendHelper
    {
        public static BlingoInkBlendFlags GetFlags(BlingoInkType ink)
        {
            if ((int)ink < 32 || (int)ink > 41)
                return BlingoInkBlendFlags.None;

            int raw = (int)ink;
            return (BlingoInkBlendFlags)(raw & 0x2F); // 0x20 base + 0x0F mask
        }

        public static bool IsBlend(BlingoInkType ink) =>
            ((int)ink & (int)BlingoInkBlendFlags.BlendBase) != 0;
    }
}

