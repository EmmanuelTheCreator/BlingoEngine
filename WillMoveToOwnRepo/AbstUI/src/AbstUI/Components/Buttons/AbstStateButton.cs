using AbstUI.Primitives;

namespace AbstUI.Components
{
    /// <summary>
    /// Engine level wrapper for a state (toggle) button.
    /// </summary>
    public class AbstStateButton : AbstInputBase<IAbstFrameworkStateButton>
    {
        /// <summary>Displayed text on the button.</summary>
        public string Text { get => _framework.Text; set => _framework.Text = value; }
        /// <summary>Icon texture displayed when the button is on.</summary>
        private IAbstUITextureUserSubscription? _textureOnSubscription;
        public IAbstTexture2D? TextureOn
        {
            get => _framework.TextureOn;
            set
            {
                _textureOnSubscription?.Release();
                _textureOnSubscription = value?.AddUser(this);
                _framework.TextureOn = value;
            }
        }
        /// <summary>Current toggle state.</summary>
        public bool IsOn { get => _framework.IsOn; set => _framework.IsOn = value; }
        private IAbstUITextureUserSubscription? _textureOffSubscription;
        public IAbstTexture2D? TextureOff
        {
            get => _framework.TextureOff;
            set
            {
                _textureOffSubscription?.Release();
                _textureOffSubscription = value?.AddUser(this);
                _framework.TextureOff = value;
            }
        }

        public override void Dispose()
        {
            _textureOnSubscription?.Release();
            _textureOffSubscription?.Release();
            _textureOnSubscription = null;
            _textureOffSubscription = null;
            _framework.TextureOn = null;
            _framework.TextureOff = null;
            base.Dispose();
        }
    }
}
