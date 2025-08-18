using LingoEngine.Blazor;
using LingoEngine.Setup;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.RegisterLingoEngine(c => c
    .WithLingoBlazorEngine()
    .SetProjectFactory<LingoEngine.Demo.TetriGrounds.Core.TetriGroundsProjectFactory>()
    .BuildAndRunProject());

var app = builder.Build();
app.Run();
