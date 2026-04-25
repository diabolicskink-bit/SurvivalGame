using System;
using System.Collections.Generic;
using Godot;
using SurvivalGame.Domain;

public partial class ActionPanel : VBoxContainer
{
    private const int ButtonFontSize = 16;

    public event Action<AvailableAction>? ActionSelected;

    public override void _Ready()
    {
        AddThemeConstantOverride("separation", 6);
    }

    public void Display(IReadOnlyList<AvailableAction> actions)
    {
        foreach (var child in GetChildren())
        {
            RemoveChild(child);
            child.QueueFree();
        }

        foreach (var action in actions)
        {
            var button = new Button
            {
                Text = action.Label,
                CustomMinimumSize = new Vector2(0, 34),
                FocusMode = Control.FocusModeEnum.None
            };
            button.AddThemeFontSizeOverride("font_size", ButtonFontSize);

            button.Pressed += () => ActionSelected?.Invoke(action);
            AddChild(button);
        }
    }
}
