using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using LingoEngine.Setup;
using LingoEngine.Blazor;

namespace LingoEngine.Demo.TetriGrounds.Blazor
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            //builder.Services.RegisterLingoEngine(c => c
            //    .WithLingoBlazorEngine()
            //    .SetProjectFactory<LingoEngine.Demo.TetriGrounds.Core.TetriGroundsProjectFactory>()
            //    .BuildAndRunProject());
            await builder.Build().RunAsync();
        }
    }
}
