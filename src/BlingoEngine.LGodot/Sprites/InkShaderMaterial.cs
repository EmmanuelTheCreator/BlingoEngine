using Godot;
using BlingoEngine.Primitives;

namespace BlingoEngine.LGodot.Sprites
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
            render_mode blend_mix;

            uniform vec4 transparent_color;
            uniform int ink_type;

            void fragment() {
                vec4 tex = texture(TEXTURE, UV);
                vec4 result = tex;

                // Matte
                if (ink_type == 8 && distance(tex.rgb, transparent_color.rgb) < 0.000001) {
                    discard;
                }
                // BackgroundTransparent
                else if (ink_type == 36 && distance(tex.rgb, transparent_color.rgb) < 0.01) {
                    discard;
                }
                // Reverse
                else if (ink_type == 2 || ink_type == 4 || ink_type == 6) {
                    result.rgb = 1.0 - tex.rgb;
                }
                // Ghost / NotGhost
                else if (ink_type == 3 || ink_type == 7 || ink_type == 9) {
                    if (ink_type == 9) result.rgb = 1.0 - tex.rgb;
                    result.a *= 0.5;
                }
                // NotTransparent
                else if (ink_type == 5) {
                    result.a = 1.0;
                }
                // Mask
                else if (ink_type == 9) {
                    result.rgb = tex.rgb; // placeholder
                }

                // Blend modes (bitflag-based: ink_type 32–41)
                else if ((ink_type & 0x20) != 0) {
                    int blend_mode = ink_type & 0x0F;

                    if (blend_mode == 0x00) {
                        result.rgb = tex.rgb * tex.a; // Blend (32)
                    }
                    else if (blend_mode == 0x01) {
                        result.rgb = min(tex.rgb + vec3(0.0), vec3(1.0)); // AddPin (clamp), SCREEN_TEXTURE not used
                    }
                    else if (blend_mode == 0x02) {
                        result.rgb = tex.rgb + vec3(0.0); // Add
                    }
                    else if (blend_mode == 0x03) {
                        result.rgb = max(tex.rgb - vec3(0.2), vec3(0.0)); // SubPin approx
                    }
                    else if (blend_mode == 0x06) {
                        result.rgb = max(tex.rgb - vec3(0.2), vec3(0.0)); // Subtract
                    }
                    else if (blend_mode == 0x07) {
                        result.rgb = min(tex.rgb, vec3(0.5)); // Darkest
                    }
                    else if (blend_mode == 0x08) {
                        result.rgb = max(tex.rgb, vec3(0.5)); // Lighten
                    }
                    else if (blend_mode == 0x09) {
                        result.rgb = tex.rgb * 0.5; // Darken approx
                    }
                }

                COLOR = result;
            }
            
            """;

                _material = new ShaderMaterial { Shader = shader };
            }

            return _material;
        }

        public static ShaderMaterial Create(BlingoInkType ink, Color transparentColor)
        {
            var mat = (ShaderMaterial)GetMaterial().Duplicate();
            mat.SetShaderParameter("ink_type", (int)ink);
            mat.SetShaderParameter("transparent_color", transparentColor);
            return mat;
        }
    }

}

