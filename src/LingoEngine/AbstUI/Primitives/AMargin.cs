namespace LingoEngine.AbstUI.Primitives
{
    /// <summary>
    /// Represents margin or padding values with left, top, right and bottom components.
    /// </summary>
    public struct AMargin
    {
        public float Left { get; set; }
        public float Top { get; set; }
        public float Right { get; set; }
        public float Bottom { get; set; }

        public AMargin(float left, float top, float right, float bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        /// <summary>Margin with all sides set to zero.</summary>
        public static readonly AMargin Zero = new(0, 0, 0, 0);
    }
}
