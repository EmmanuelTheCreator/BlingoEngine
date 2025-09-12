using System.Reflection;
using AbstUI.SDL2.Core;
using FluentAssertions;
using Xunit;

namespace AbstUI.SDLTest;

public class ZIndexEventOrderTests
{
    [Fact]
    public void Activate_SortsByZIndex()
    {
        var focus = new SdlFocusManager();
        var container = new AbstSDLComponentContainer(focus);

        var low = new AbstSDLComponentContext(container);
        low.SetZIndex(0);
        var high = new AbstSDLComponentContext(container);
        high.SetZIndex(10);

        var field = typeof(AbstSDLComponentContainer)
            .GetField("_activeComponents", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var list = (List<AbstSDLComponentContext>)field.GetValue(container)!;

        list.Should().ContainInOrder(low, high);
    }
}
