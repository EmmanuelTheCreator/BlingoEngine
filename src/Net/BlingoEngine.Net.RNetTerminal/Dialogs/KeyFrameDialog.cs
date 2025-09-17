using BlingoEngine.Net.RNetTerminal.Views;
using System.Collections.Generic;
using System.Globalization;
using Terminal.Gui.App;
using Terminal.Gui.Views;
using static BlingoEngine.Net.RNetTerminal.Views.ScoreView;

namespace BlingoEngine.Net.RNetTerminal.Dialogs
{
    internal class KeyFrameDialog
    {
        public static void EditKeyframeDialog(SpriteBlock sprite, int frame)
        {
            if (!sprite.Keyframes.TryGetValue(frame, out var props))
            {
                props = new Dictionary<string, double>();
                sprite.Keyframes[frame] = props;
            }
            var rows = new List<(CheckBox cb, TextField field, string name)>();
            for (var i = 0; i < ScoreView.TweenProperties.Length; i++)
            {
                var name = ScoreView.TweenProperties[i];
                var cb = new CheckBox() { Text = name, X = 1, Y = i + 1, CheckedState = props.ContainsKey(name).ToCheckedSTate() };
                var field = new TextField()
                {
                    Text = props.TryGetValue(name, out var val) ? val.ToString(CultureInfo.InvariantCulture) : "0",
                    X = 15,
                    Y = i + 1,
                    Width = 10,
                    Visible = cb.CheckedState.ToBool()
                };

                cb.CheckedStateChanged += (_, _) =>
                {
                    field.Visible = cb.CheckedState.ToBool();
                    if (cb.CheckedState.ToBool())
                    {
                        field.SetFocus();
                    }
                };
                rows.Add((cb, field, name));
            }
            var ok = RUI.NewButton("Ok", true, () =>
            {
                var result = new Dictionary<string, double>();
                foreach (var (cb, field, name) in rows)
                {
                    if (cb.CheckedState.ToBool() && double.TryParse(field.Text.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var val))
                    {
                        result[name] = val;
                    }
                }
                sprite.Keyframes[frame] = result;
                Application.RequestStop();
            });
            var dialog = RUI.NewDialog("Keyframe", 40, TweenProperties.Length + 4, ok);
            foreach (var (cb, field, _) in rows)
            {
                dialog.Add(cb, field);
            }
            dialog.Add(RUI.NewLabel("Use Space to toggle", 1, TweenProperties.Length + 1));
            rows[0].cb.SetFocus();
            Application.Run(dialog);
        }
    }
}

