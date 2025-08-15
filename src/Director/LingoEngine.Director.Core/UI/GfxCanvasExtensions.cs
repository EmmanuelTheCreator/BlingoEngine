using LingoEngine.Director.Core.Styles;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using LingoEngine.AbstUI.Primitives;


namespace LingoEngine.Director.Core.UI
{
    public static class GfxCanvasExtensions
    {
        public static LingoGfxCanvas DrawHLine(this LingoGfxCanvas canvas, float x, float y, float width)
        {
            canvas.DrawLine(new APoint(x, y), new APoint(x+ width, y), DirectorColors.LineDark);
            canvas.DrawLine(new APoint(x, y + 1), new APoint(x+ width, y + 1), DirectorColors.LineLight);
            return canvas;
        } 
        public static LingoGfxCanvas DrawHLineR(this LingoGfxCanvas canvas, float x, float y, float width)
        {
            canvas.DrawLine(new APoint(x, y), new APoint(x+ width, y), DirectorColors.LineLight);
            canvas.DrawLine(new APoint(x, y + 1), new APoint(x+ width, y + 1), DirectorColors.LineDark);
            return canvas;
        }
       
        public static LingoGfxCanvas DrawVLine(this LingoGfxCanvas canvas,  float x, float y, float height)
        {
            canvas.DrawLine(new APoint(x, y), new APoint(x, y+height), DirectorColors.LineLight, 1);
            canvas.DrawLine(new APoint(x, y+ 1), new APoint(x+1, y+height), DirectorColors.LineDark, 1);
            return canvas;
        }
        public static LingoGfxCanvas DrawVLineR(this LingoGfxCanvas canvas,  float x, float y, float height)
        {
            canvas.DrawLine(new APoint(x, y), new APoint(x, y+height), DirectorColors.LineDark, 1);
            canvas.DrawLine(new APoint(x, y+ 1), new APoint(x+1, y+height), DirectorColors.LineLight, 1);
            return canvas;
        }
    }
}
