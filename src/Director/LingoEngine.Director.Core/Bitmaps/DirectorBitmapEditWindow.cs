using AbstUI.Commands;
using AbstUI.Windowing;
using LingoEngine.Director.Core.Bitmaps.Commands;
using LingoEngine.Director.Core.UI;

namespace LingoEngine.Director.Core.Bitmaps
{
    public class DirectorBitmapEditWindow : DirectorWindow<IDirFrameworkBitmapEditWindow>,
            IAbstCommandHandler<PainterToolSelectCommand>,
            IAbstCommandHandler<PainterDrawPixelCommand>,
            IAbstCommandHandler<PainterFillCommand>
    {
        public DirectorBitmapEditWindow(IServiceProvider serviceProvider) : base(serviceProvider, DirectorMenuCodes.PictureEditWindow)
        {
            Width = 800;
            Height = 500;
            MinimumWidth = 200;
            MinimumHeight = 150;
            X = 20;
            Y = 120;
        }

        protected override void OnInit(IAbstFrameworkWindow frameworkWindow)
        {
            base.OnInit(frameworkWindow);
            Title = "Bitmap Editor";
        }
        public bool CanExecute(PainterToolSelectCommand command) => true;

        public bool Handle(PainterToolSelectCommand command) => Framework.SelectTheTool(command.Tool);

        public bool CanExecute(PainterDrawPixelCommand command) => true;

        public bool Handle(PainterDrawPixelCommand command) => Framework.DrawThePixel(command.X, command.Y);

        public bool CanExecute(PainterFillCommand command) => false;

        public bool Handle(PainterFillCommand command) => true;
    }
}
