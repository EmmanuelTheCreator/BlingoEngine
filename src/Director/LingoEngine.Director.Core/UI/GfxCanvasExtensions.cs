using LingoEngine.Director.Core.Styles;
using LingoEngine.Primitives;
using AbstUI.Primitives;
using AbstUI.Components.Graphics;


namespace LingoEngine.Director.Core.UI
{
    public static class GfxCanvasExtensions
    {
        public static AbstGfxCanvas DrawHLine(this AbstGfxCanvas canvas, float x, float y, float width)
        {
            canvas.DrawLine(new APoint(x, y), new APoint(x+ width, y), DirectorColors.LineDark);
            canvas.DrawLine(new APoint(x, y + 1), new APoint(x+ width, y + 1), DirectorColors.LineLight);
            return canvas;
        } 
        public static AbstGfxCanvas DrawHLineR(this AbstGfxCanvas canvas, float x, float y, float width)
        {
            canvas.DrawLine(new APoint(x, y), new APoint(x+ width, y), DirectorColors.LineLight);
            canvas.DrawLine(new APoint(x, y + 1), new APoint(x+ width, y + 1), DirectorColors.LineDark);
            return canvas;
        }
       
        public static AbstGfxCanvas DrawVLine(this AbstGfxCanvas canvas,  float x, float y, float height)
        {
            canvas.DrawLine(new APoint(x, y), new APoint(x, y+height), DirectorColors.LineLight, 1);
            canvas.DrawLine(new APoint(x, y+ 1), new APoint(x+1, y+height), DirectorColors.LineDark, 1);
            return canvas;
        }
        public static AbstGfxCanvas DrawVLineR(this AbstGfxCanvas canvas,  float x, float y, float height)
        {
            canvas.DrawLine(new APoint(x, y), new APoint(x, y+height), DirectorColors.LineDark, 1);
            canvas.DrawLine(new APoint(x, y+ 1), new APoint(x+1, y+height), DirectorColors.LineLight, 1);
            return canvas;
        }
    }
}
