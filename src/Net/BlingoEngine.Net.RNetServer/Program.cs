using BlingoEngine.Net.RNetServer;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ProjectRegistry>();
builder.Services.AddControllers();
builder.Services.AddSignalR();

var app = builder.Build();
app.MapControllers();
app.MapHub<ProjectRelayHub>("/rnet");
app.Run();

