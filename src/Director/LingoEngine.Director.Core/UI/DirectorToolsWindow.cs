namespace LingoEngine.Director.Core.UI
{
    public class DirectorToolsWindow : DirectorWindow<IDirFrameworkToolsWindow>
    {
        public DirectorToolsWindow(IServiceProvider serviceProvider) : base(serviceProvider, DirectorMenuCodes.ToolsWindow)
        {
            Width = 60;
            Height = 300;
            MinimumWidth = 60;
            MinimumHeight = 200;
            X = 2;
            Y = 22;
        }
    }
}
