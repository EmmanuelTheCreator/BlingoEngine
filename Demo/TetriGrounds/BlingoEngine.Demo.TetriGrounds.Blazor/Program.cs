using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlingoEngine.Setup;
using BlingoEngine.Blazor;

namespace BlingoEngine.Demo.TetriGrounds.Blazor
{
    public class Program
    {
        public static IBlingoEngineRegistration? BlingoEngineReg { get; private set; }

        public static async Task Main(string[] args)
        {
            IBlingoEngineRegistration blingoEngineReg = null;
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.RegisterBlingoEngine(c =>
            {
                blingoEngineReg = c
                      .WithBlingoBlazorEngine()
                     .SetProjectFactory<BlingoEngine.Demo.TetriGrounds.Core.TetriGroundsProjectFactory>()
                    .BuildDelayed();
            });
            var app = builder.Build();
            //BuildIt = () =>
            //{
                try
                {
                    BlingoEngineReg = blingoEngineReg;
                    blingoEngineReg!.Build(app.Services,false);
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

