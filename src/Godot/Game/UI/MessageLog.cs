using System.Collections.Generic;
using Godot;

public partial class MessageLog : VBoxContainer
{
    private const int MaxVisibleMessages = 6;
    private readonly List<string> _messages = new();

    public override void _Ready()
    {
        AddThemeConstantOverride("separation", 4);
    }

    public void AddMessage(string message)
    {
        _messages.Add(message);

        while (_messages.Count > MaxVisibleMessages)
        {
            _messages.RemoveAt(0);
        }

        Refresh();
    }

    private void Refresh()
    {
        foreach (var child in GetChildren())
        {
            RemoveChild(child);
            child.QueueFree();
        }

        foreach (var message in _messages)
        {
            var label = new Label
            {
                Text = message,
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            label.AddThemeFontSizeOverride("font_size", 14);
            label.AddThemeColorOverride("font_color", new Color(0.72f, 0.79f, 0.75f));
            AddChild(label);
        }
    }
}
