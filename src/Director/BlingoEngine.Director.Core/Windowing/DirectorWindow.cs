using AbstUI.Windowing;
using BlingoEngine.FrameworkCommunication;
using Microsoft.Extensions.DependencyInjection;

public class DirectorWindow<TFrameworkWindow> : AbstWindow<TFrameworkWindow>
    where TFrameworkWindow : IAbstFrameworkWindow
{
    protected IBlingoFrameworkFactory _factory;
    public IBlingoFrameworkFactory Factory => _factory;

    public DirectorWindow(IServiceProvider serviceProvider,string windowCode) : base(serviceProvider, windowCode )
    {
        _factory = serviceProvider.GetRequiredService<IBlingoFrameworkFactory>();
    }
}

