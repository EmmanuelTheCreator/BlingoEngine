using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace LingoEngine.Net.RNetServer.Controllers;

[ApiController]
[Route("/")]
public class HomeController : ControllerBase
{
    private readonly ProjectRegistry _registry;

    public HomeController(ProjectRegistry registry) => _registry = registry;

    [HttpGet]
    public ContentResult Index()
    {
        var projects = _registry.Projects.Select(p => $"{p.Key}:{p.Value.Clients.Count}");
        var body = $"<html><head><title>LingoEngine Remote Net Server</title></head><body>Running=OK<br/>Projects= {string.Join(", ", projects)}</body></html>";
        return Content(body, "text/html");
    }
}
