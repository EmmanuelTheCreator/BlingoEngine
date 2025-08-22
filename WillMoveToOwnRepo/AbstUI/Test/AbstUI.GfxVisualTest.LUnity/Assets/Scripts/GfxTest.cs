using Microsoft.Extensions.DependencyInjection;
using UnityEngine;
using AbstUI.Components;
using AbstUI.GfxVisualTest;
using AbstUI.LUnity;
using LingoEngine.SDL2.GfxVisualTest;

namespace AbstUI.GfxVisualTest.LUnity;

public class GfxTest : MonoBehaviour
{
    // Fields
    private ServiceProvider _serviceProvider = null!;

    // Methods
    private void Start()
    {
        Screen.SetResolution(800, 600, FullScreenMode.Windowed);

        var services = new ServiceCollection();
        services.WithAbstUIUnity();
        services.WithAbstUI(w => w.AddSingletonWindow<GfxTestWindow, UnityTestWindow>(GfxTestWindow.MyWindowCode));
        services.SetupGfxTest();

        _serviceProvider = services.BuildServiceProvider();
        _serviceProvider.WithAbstUIUnity();
        _serviceProvider.SetupGfxTest();

        var factory = _serviceProvider.GetRequiredService<IAbstComponentFactory>();
        var root = GfxTestScene.Build(factory);
        if (root.FrameworkObj.FrameworkNode is GameObject go)
        {
            go.transform.SetParent(gameObject.transform, false);
        }
    }
}
