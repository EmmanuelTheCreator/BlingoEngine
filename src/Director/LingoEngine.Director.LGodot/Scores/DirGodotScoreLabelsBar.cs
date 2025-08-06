//using Godot;
//using LingoEngine.Director.Core.Scores;
//using LingoEngine.Movies;
//using LingoEngine.Director.LGodot.Styles;
//using LingoEngine.Gfx;

//namespace LingoEngine.Director.LGodot.Scores;

//internal partial class DirGodotScoreLabelsBar : Control
//{
//    private readonly DirScoreLabelsBar _labelsBar;

//    public DirGodotScoreLabelsBar(DirScoreLabelsBar labelsBar,DirectorGodotStyle godotStyle)
//    {
//        _labelsBar = labelsBar;
//        _labelsBar.RequestRedraw = () => QueueRedraw();
//        AddChild((Node)_labelsBar.ScollingPanel.FrameworkObj.FrameworkNode);
//        //var editNode = (Node)_labelsBar.EditField.FrameworkObj.FrameworkNode;
//        //AddChild(editNode);
//        //if (editNode is LineEdit le)
//          //  le.Theme = godotStyle.GetLineEditTheme();
//        MouseFilter = MouseFilterEnum.Ignore;
//    }
//    public LingoGfxPanel LabelsFixPanel => _labelsBar.FixPanel;
   

   


//    public override void _Draw()
//    {
//        _labelsBar.Draw();
//        Size = new Vector2(_labelsBar.ScrollingWidth, _labelsBar.ScollingHeight);
//        CustomMinimumSize = Size;
//    }

//}
