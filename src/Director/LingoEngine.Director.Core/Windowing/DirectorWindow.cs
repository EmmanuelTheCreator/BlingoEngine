using AbstUI.Windowing;
using LingoEngine.FrameworkCommunication;
using Microsoft.Extensions.DependencyInjection;

public class DirectorWindow<TFrameworkWindow> : AbstWindow<TFrameworkWindow>
    where TFrameworkWindow : IAbstFrameworkWindow
{
    protected ILingoFrameworkFactory _factory;
    public ILingoFrameworkFactory Factory => _factory;

    public DirectorWindow(IServiceProvider serviceProvider,string windowCode) : base(serviceProvider, windowCode )
    {
        _factory = serviceProvider.GetRequiredService<ILingoFrameworkFactory>();
    }
}
