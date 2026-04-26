using System;
using Godot;
using SurvivalGame.Domain;

public partial class PlayerController : Node
{
    public event Action<GridOffset>? MoveRequested;

    public override void _Ready()
    {
        SetProcessUnhandledInput(true);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is not InputEventKey keyEvent || !keyEvent.Pressed || keyEvent.Echo)
        {
            return;
        }

        var direction = DirectionFromKey(keyEvent.Keycode);
        if (direction == GridOffset.Zero)
        {
            return;
        }

        GetViewport().SetInputAsHandled();
        MoveRequested?.Invoke(direction);
    }

    private static GridOffset DirectionFromKey(Key key)
    {
        return key switch
        {
            Key.W or Key.Up => GridOffset.Up,
            Key.S or Key.Down => GridOffset.Down,
            Key.A or Key.Left => GridOffset.Left,
            Key.D or Key.Right => GridOffset.Right,
            _ => GridOffset.Zero
        };
    }
}
