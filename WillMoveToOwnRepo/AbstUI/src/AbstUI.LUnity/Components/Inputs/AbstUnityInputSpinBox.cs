using AbstUI.Components.Inputs;
using AbstUI.FrameworkCommunication;

namespace AbstUI.LUnity.Components.Inputs;

/// <summary>
/// Unity implementation of <see cref="IAbstFrameworkSpinBox"/>.
/// </summary>
internal class AbstUnityInputSpinBox : AbstUnityInputNumber<float>, IAbstFrameworkSpinBox, IFrameworkFor<AbstInputSpinBox>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AbstUnityInputSpinBox"/> class.
    /// </summary>
    public AbstUnityInputSpinBox() : base()
    {
    }
}
