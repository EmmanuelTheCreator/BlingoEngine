using System;

namespace BlingoEngine.Core
{
    /// <summary>
    /// Lingo Service Provider interface.
    /// </summary>
    public interface IBlingoServiceProvider : IServiceProvider
    {
        void SetServiceProvider(IServiceProvider serviceProvider);
    }

    public class BlingoServiceProvider : IBlingoServiceProvider
    {
        private IServiceProvider? _serviceProvider;

        public void SetServiceProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public object? GetService(Type serviceType)
        {
            if (_serviceProvider == null)
                throw new InvalidOperationException("Service provider is not set.");
            return _serviceProvider.GetService(serviceType);
        }
    }
}


