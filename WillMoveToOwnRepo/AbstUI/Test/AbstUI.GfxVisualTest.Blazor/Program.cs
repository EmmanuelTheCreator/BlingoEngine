using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using AbstUI.Blazor;

namespace AbstUI.GfxVisualTest.Blazor
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.WithAbstUIBlazor();// r => 
            //r.Register<GfxTestWindow,BlazorTestWindow>
            //);

            var app = builder.Build();
            app.Services.WithAbstUIBlazor();
            await app.RunAsync();
        }
    }
}
