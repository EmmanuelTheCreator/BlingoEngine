using LingoEngine.IO.Data.DTO.Sprites;
using System;
using Terminal.Gui.App;
using Terminal.Gui.Views;

namespace LingoEngine.Net.RNetTerminal.Dialogs
{
    internal class SpriteEditDialog
    {
        public static void EditSpriteDialog(string title, Lingo2DSpriteDTO? sprite,Action<(int Begin, int End, int LocH, int LocV, int CastLibNum, int NumberInCast, float Width, float Height)> onOk)
        {
            var colX1 = 1;
            var colX2 = 14;
            var colX3 = 22;
            var colX4 = 35;
            var begin = new TextField { Text = sprite !=null? sprite.BeginFrame.ToString(): "1", X = colX2, Y = 1, Width = 5 };
            var end = new TextField { Text = sprite !=null? sprite.EndFrame.ToString():"1", X = colX4, Y = 1, Width = 5 };

            var locH = new TextField { Text = sprite !=null? sprite.LocH.ToString():"0", X = colX2, Y = 3, Width = 5 };
            var locV = new TextField { Text = sprite !=null? sprite.LocV.ToString():"0", X = colX4, Y = 3, Width = 5 };

            var width = new TextField { Text = sprite !=null? sprite.Width.ToString():"0", X = colX2, Y = 5, Width = 5 };
            var height = new TextField { Text = sprite !=null? sprite.Height.ToString():"0", X = colX4, Y = 5, Width = 5 };

            var castLib = new TextField { Text = sprite !=null && sprite.Member  != null? sprite.Member.CastLibNum.ToString():"1", X = colX2, Y = 7, Width = 5 };
            var memberNum = new TextField { Text = sprite != null && sprite.Member != null ? sprite.Member.MemberNum.ToString() : "1", X = colX4, Y = 7, Width = 5 };
            var ok = RUI.NewButton("Ok", true, () =>
            {
                if (int.TryParse(begin.Text.ToString(), out var b) &&
                    int.TryParse(end.Text.ToString(), out var e) &&
                    int.TryParse(castLib.Text.ToString(), out var cast) &&
                    int.TryParse(memberNum.Text.ToString(), out var mem))
                {
                    var x = int.TryParse(locH.Text.ToString(), out var lh) ? lh : 0;
                    var y = int.TryParse(locV.Text.ToString(), out var lv) ? lv : 0;
                    var w = int.TryParse(width.Text.ToString(), out var ww) ? ww : 0;
                    var h = int.TryParse(height.Text.ToString(), out var hh) ? hh : 0;
                    onOk((b, e, x, y, cast, mem,w,h));
                }
                Application.RequestStop();
            });
            var dialog = RUI.NewDialog(title, 50, 15, ok);
            dialog.Add(
                new Label { Text = "Begin:", X = colX1, Y = 1 }, begin,
                new Label { Text = "End:", X = colX3, Y = 1 }, end,
                new Label { Text = "LocH:", X = colX1, Y = 3 }, locH,
                new Label { Text = "LocV:", X = colX3, Y = 3 }, locV,
                new Label { Text = "Width:", X = colX1, Y = 5 }, width,
                new Label { Text = "Heiht:", X = colX3, Y = 5 }, height,
                new Label { Text = "CastLib:", X = colX1, Y = 7 }, castLib,
                new Label { Text = "MemberNum:", X = colX3, Y = 7 }, memberNum);
            begin.SetFocus();
            Application.Run(dialog);
        }
    }
}
