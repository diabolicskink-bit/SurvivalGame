using Godot;

public partial class MainMenu : Control
{
    private const string GameSessionShellScenePath = "res://src/Godot/Game/GameSessionShell.tscn";

    public override void _Ready()
    {
        BuildMenu();
    }

    private void BuildMenu()
    {
        var background = new ColorRect
        {
            Color = new Color(0.052f, 0.062f, 0.075f)
        };
        background.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(background);

        var center = new CenterContainer();
        center.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(center);

        var panel = new PanelContainer
        {
            CustomMinimumSize = new Vector2(460, 310)
        };
        panel.AddThemeStyleboxOverride("panel", CreatePanelStyle());
        center.AddChild(panel);

        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 34);
        margin.AddThemeConstantOverride("margin_top", 30);
        margin.AddThemeConstantOverride("margin_right", 34);
        margin.AddThemeConstantOverride("margin_bottom", 30);
        panel.AddChild(margin);

        var stack = new VBoxContainer
        {
            Alignment = BoxContainer.AlignmentMode.Center
        };
        stack.AddThemeConstantOverride("separation", 14);
        margin.AddChild(stack);

        var title = new Label
        {
            Text = "Survival Roguelike Prototype",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        title.AddThemeFontSizeOverride("font_size", 30);
        title.AddThemeColorOverride("font_color", new Color(0.92f, 0.94f, 0.9f));
        stack.AddChild(title);

        var subtitle = new Label
        {
            Text = "Overworld travel shell",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        subtitle.AddThemeFontSizeOverride("font_size", 18);
        subtitle.AddThemeColorOverride("font_color", new Color(0.62f, 0.72f, 0.7f));
        stack.AddChild(subtitle);

        stack.AddChild(new Control { CustomMinimumSize = new Vector2(1, 16) });

        var newRunButton = CreateMenuButton("New Run");
        newRunButton.Name = "NewRunButton";
        newRunButton.Pressed += OnNewRunPressed;
        stack.AddChild(newRunButton);

        var quitButton = CreateMenuButton("Quit");
        quitButton.Name = "QuitButton";
        quitButton.Pressed += OnQuitPressed;
        stack.AddChild(quitButton);
    }

    private static Button CreateMenuButton(string text)
    {
        return new Button
        {
            Text = text,
            CustomMinimumSize = new Vector2(260, 44),
            FocusMode = Control.FocusModeEnum.All
        };
    }

    private static StyleBoxFlat CreatePanelStyle()
    {
        return new StyleBoxFlat
        {
            BgColor = new Color(0.092f, 0.107f, 0.12f),
            BorderColor = new Color(0.24f, 0.32f, 0.3f),
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            CornerRadiusTopLeft = 8,
            CornerRadiusTopRight = 8,
            CornerRadiusBottomRight = 8,
            CornerRadiusBottomLeft = 8
        };
    }

    private void OnNewRunPressed()
    {
        GetTree().ChangeSceneToFile(GameSessionShellScenePath);
    }

    private void OnQuitPressed()
    {
        GetTree().Quit();
    }
}
