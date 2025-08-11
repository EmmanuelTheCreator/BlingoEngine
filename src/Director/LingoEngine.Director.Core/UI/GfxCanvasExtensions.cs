using LingoEngine.Director.Core.Styles;
using LingoEngine.Gfx;
using LingoEngine.Primitives;


namespace LingoEngine.Director.Core.UI
{
    public static class GfxCanvasExtensions
    {
        public static LingoGfxCanvas DrawHLine(this LingoGfxCanvas canvas, float x, float y, float width)
        {
            canvas.DrawLine(new LingoPoint(x, y), new LingoPoint(x+ width, y), DirectorColors.LineDark);
            canvas.DrawLine(new LingoPoint(x, y + 1), new LingoPoint(x+ width, y + 1), DirectorColors.LineLight);
            return canvas;
        } 
        public static LingoGfxCanvas DrawHLineR(this LingoGfxCanvas canvas, float x, float y, float width)
        {
            canvas.DrawLine(new LingoPoint(x, y), new LingoPoint(x+ width, y), DirectorColors.LineLight);
            canvas.DrawLine(new LingoPoint(x, y + 1), new LingoPoint(x+ width, y + 1), DirectorColors.LineDark);
            return canvas;
        }
       
        public static LingoGfxCanvas DrawVLine(this LingoGfxCanvas canvas,  float x, float y, float height)
        {
            canvas.DrawLine(new LingoPoint(x, y), new LingoPoint(x, y+height), DirectorColors.LineLight, 1);
            canvas.DrawLine(new LingoPoint(x, y+ 1), new LingoPoint(x+1, y+height), DirectorColors.LineDark, 1);
            return canvas;
        }
        public static LingoGfxCanvas DrawVLineR(this LingoGfxCanvas canvas,  float x, float y, float height)
        {
            canvas.DrawLine(new LingoPoint(x, y), new LingoPoint(x, y+height), DirectorColors.LineDark, 1);
            canvas.DrawLine(new LingoPoint(x, y+ 1), new LingoPoint(x+1, y+height), DirectorColors.LineLight, 1);
            return canvas;
        }
    }
}
