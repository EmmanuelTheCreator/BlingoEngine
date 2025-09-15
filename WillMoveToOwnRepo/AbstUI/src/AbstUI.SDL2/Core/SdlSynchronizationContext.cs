namespace AbstUI.SDL2.Core
{
    public class SdlSynchronizationContext : SynchronizationContext
    {
        private readonly SdlDispatcher _dispatcher;

        public SdlSynchronizationContext(SdlDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public override void Post(SendOrPostCallback d, object? state)
        {
            _dispatcher.Post(() => d(state));
        }

        public override void Send(SendOrPostCallback d, object? state)
        {
            // Blocks until done (danger: deadlock if called on SDL thread itself)
            using var ev = new ManualResetEventSlim();
            Exception? ex = null;
            _dispatcher.Post(() =>
            {
                try { d(state); }
                catch (Exception e) { ex = e; }
                finally { ev.Set(); }
            });
            ev.Wait();
            if (ex != null) throw ex;
        }
    }

}
