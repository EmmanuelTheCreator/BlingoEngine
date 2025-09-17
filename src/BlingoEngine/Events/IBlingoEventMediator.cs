namespace BlingoEngine.Events
{
    /// <summary>
    /// Lingo Event Mediator interface.
    /// </summary>
    public interface IBlingoEventMediator
    {
        /// <summary>
        /// Subscribe an object to relevant events.
        /// </summary>
        /// <param name="ms">Object implementing one or more event interfaces.</param>
        /// <param name="priority">Optional priority. Lower values are executed first.</param>
        void Subscribe(object listener, int priority = 5000, bool ignoreMouse = false);
        void Unsubscribe(object listener, bool ignoreMouse = false);
        void Clear(string? preserveNamespaceFragment = null);
    }
}

