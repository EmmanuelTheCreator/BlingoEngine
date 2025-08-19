namespace AbstUI.FrameworkCommunication
{
    /// <summary>
    /// Marker interface linking a framework implementation to a Abst type.
    /// </summary>
    /// <typeparam name="T">The Abst type this framework implementation supports.</typeparam>
    public interface IFrameworkFor<T> {
    }
    public interface IFrameworkForInitializable<T> : IFrameworkFor<T>
    {
        void Init(T instance);
    }
}
