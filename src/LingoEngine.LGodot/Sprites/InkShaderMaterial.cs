using Godot;
using LingoEngine.Primitives;

namespace LingoEngine.LGodot.Sprites
{
    public static class InkShaderMaterial
    {
        private static ShaderMaterial? _material;
        private static ShaderMaterial GetMaterial()
        {
            if (_material == null)
            {
                var shader = new Shader();
                shader.Code = """
                shader_type canvas_item;
            render_mode blend_add;

            uniform vec4 transparent_color;
            uniform int ink_type = 0;

            void fragment() {
                vec4 tex = texture(TEXTURE, UV);

                if (ink_type == 1 && distance(tex.rgb, transparent_color.rgb) < 0.01)
                    discard;

                    COLOR = tex;
                }
            """;

                _material = new ShaderMaterial { Shader = shader };
            }

            return _material;
        }

        public static ShaderMaterial Create(LingoInkType ink, Color transparentColor)
        {
            var mat = (ShaderMaterial)GetMaterial().Duplicate();
            mat.SetShaderParameter("ink_type", (int)ink);
            mat.SetShaderParameter("transparent_color", transparentColor);
            return mat;
        }
    }

}
