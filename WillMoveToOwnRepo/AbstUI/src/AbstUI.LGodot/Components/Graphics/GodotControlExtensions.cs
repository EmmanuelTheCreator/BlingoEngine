using System;
using AbstUI.Bitmaps;
using AbstUI.LGodot.Bitmaps;
using Godot;

namespace AbstUI.LGodot.Components.Graphics;

public static class GodotControlExtensions
{
    public static IAbstTexture2D CreateAbstTexture(this Control control, string? name = null)
    {
        var sizeI = new Vector2I((int)control.Size.X, (int)control.Size.Y);

        var holder = new Node { Name = "__tmp_vp_holder" + name };
        var tree = Engine.GetMainLoop() as SceneTree ?? throw new InvalidOperationException("No SceneTree available.");
        tree.Root.AddChild(holder);

        var vp = new SubViewport
        {
            Disable3D = true,
            TransparentBg = true,
            RenderTargetUpdateMode = SubViewport.UpdateMode.Once,
            Size = sizeI
        };
        holder.AddChild(vp);

        var clone = (Control)control.Duplicate((int)DuplicateFlags.UseInstantiation);
        clone.Position = Vector2.Zero;
        clone.Size = control.Size;
        vp.AddChild(clone);

        RenderingServer.ForceDraw();

        using var img = vp.GetTexture().GetImage();
        img.Convert(Image.Format.Rgba8);
        var tex = ImageTexture.CreateFromImage(img);

        clone.QueueFree();
        vp.QueueFree();
        holder.QueueFree();

        return new AbstGodotTexture2D(tex, name ?? $"{control.Name}_Snapshot");
    }
}

