using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using LingoEngine.Setup;
using LingoEngine.Blazor;

namespace LingoEngine.Demo.TetriGrounds.Blazor
{
    public class Program
    {
        public static ILingoEngineRegistration? LingoEngineReg { get; private set; }

        public static async Task Main(string[] args)
        {
            ILingoEngineRegistration lingoEngineReg = null;
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.RegisterLingoEngine(c =>
            {
                lingoEngineReg = c
                      .WithLingoBlazorEngine()
                     .SetProjectFactory<LingoEngine.Demo.TetriGrounds.Core.TetriGroundsProjectFactory>()
                    .BuildDelayed();
            });
            var app = builder.Build();
            //BuildIt = () =>
            //{
                try
                {
                    LingoEngineReg = lingoEngineReg;
                    lingoEngineReg!.Build(app.Services,false);
                }
                catch (Exception ex)
                {

                    throw ex;
                }
           // };

            await app.RunAsync();
        }
    }
}
