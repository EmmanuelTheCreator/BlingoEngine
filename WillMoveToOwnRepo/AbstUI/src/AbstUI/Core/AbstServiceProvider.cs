using System;
namespace AbstUI.Core;

public interface IAbstServiceProvider : IServiceProvider
{
    void SetServiceProvider(IServiceProvider serviceProvider);
}

public class AbstServiceProvider : IAbstServiceProvider
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
