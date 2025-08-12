using System;

namespace LingoEngine.Core
{
    public interface ILingoServiceProvider : IServiceProvider
    {
        void SetServiceProvider(IServiceProvider serviceProvider);
    }

    public class LingoServiceProvider : ILingoServiceProvider
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
