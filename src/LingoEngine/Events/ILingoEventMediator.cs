namespace LingoEngine.Events
{
    public interface ILingoEventMediator
    {
        /// <summary>
        /// Subscribe an object to relevant events.
        /// </summary>
        /// <param name="ms">Object implementing one or more event interfaces.</param>
        /// <param name="priority">Optional priority. Lower values are executed first.</param>
        void Subscribe(object listener, int priority = 5000);
        void Unsubscribe(object listener);
        void Clear(string? preserveNamespaceFragment = null);
    }
}
