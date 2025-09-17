namespace BlingoEngine.Sprites
{
    internal interface IBlingoSpriteEventHandler
    {

        /// <summary>
        ///  Triggered when the sprite receives focus
        /// </summary>
        void RaiseFocus();
        /// <summary>
        /// Triggered when the sprite loses focus
        /// </summary>
        void RaiseBlur();
    }
}

