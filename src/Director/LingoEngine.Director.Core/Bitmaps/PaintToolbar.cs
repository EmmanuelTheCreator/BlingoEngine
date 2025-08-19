using AbstUI.Primitives;
using AbstUI.Commands;
using LingoEngine.Director.Core.Bitmaps.Commands;
using LingoEngine.Director.Core.Icons;
using LingoEngine.Director.Core.UI;
using LingoEngine.FrameworkCommunication;

namespace LingoEngine.Director.Core.Bitmaps;

public class PaintToolbar : DirectorToolbar<PainterToolType>
{

    public AColor SelectedColor { get; private set; } = new AColor(0, 0, 0);
    public PaintToolbar(IDirectorIconManager iconManager, IAbstCommandManager commandManager, ILingoFrameworkFactory factory) : base("PaintToolbarRoot", iconManager, commandManager, factory)
    {
        AddToolButton(DirectorIcon.Pencil);
        AddToolButton(DirectorIcon.PaintBrush);
        AddToolButton(DirectorIcon.Eraser);
        AddToolButton(DirectorIcon.PaintLasso);
        AddToolButton(DirectorIcon.RectangleSelect);
        AddToolButton(DirectorIcon.PaintBucket);
        AddColorPickerForeground(AColors.Black);
        AddColorPickerBackground(AColors.White);

        SelectTool(PainterToolType.Pencil);
    }

    protected void AddToolButton(DirectorIcon icon) => AddToolButton(icon, tool => new PainterToolSelectCommand(tool));
    protected override PainterToolType ConvertToTool(DirectorIcon icon)
    {
        return icon switch
        {
            DirectorIcon.Pencil => PainterToolType.Pencil,
            DirectorIcon.PaintBrush => PainterToolType.PaintBrush,
            DirectorIcon.Eraser => PainterToolType.Eraser,
            DirectorIcon.PaintLasso => PainterToolType.SelectLasso,
            DirectorIcon.RectangleSelect => PainterToolType.SelectRectangle,
            DirectorIcon.ColorPicker => PainterToolType.ColorPicker,
            DirectorIcon.PaintBucket => PainterToolType.Fill,
            _ => throw new ArgumentOutOfRangeException(nameof(icon), icon.ToString())
        };
    }
    private void AddColorPickerForeground(AColor color) 
        => AddColorPicker(c => new PainterChangeForegroundColorCommand(c), color);

    private void AddColorPickerBackground(AColor color) 
        => AddColorPicker(c => new PainterChangeBackgroundColorCommand(c), color);

    protected void AddColorPicker(Func<AColor, IAbstCommand> toCommand, AColor color)
    {
        var picker = _factory.CreateColorPicker("PaintColorPicker", color =>
        {
            SelectedColor = color;
            _commandManager.Handle(toCommand(color));
        });
        picker.Width = 20;
        picker.Height = 20;
        picker.Color = color;
        _container.AddItem(picker);
    }


}
