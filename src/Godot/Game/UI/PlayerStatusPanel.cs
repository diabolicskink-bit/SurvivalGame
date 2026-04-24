using System.Globalization;
using Godot;
using SurvivalGame.Domain;

public partial class PlayerStatusPanel : VBoxContainer
{
    private const int RowFontSize = 15;

    public override void _Ready()
    {
        AddThemeConstantOverride("separation", 3);
    }

    public void Display(PlayerVitals vitals)
    {
        ClearRows();

        AddRow("Health", FormatMeter(vitals.Health));
        AddRow("Hunger", FormatMeter(vitals.Hunger));
        AddRow("Thirst", FormatMeter(vitals.Thirst));
        AddRow("Fatigue", FormatMeter(vitals.Fatigue));
        AddRow("Sleep Debt", FormatMeter(vitals.SleepDebt));
        AddRow("Pain", FormatMeter(vitals.Pain));
        AddRow("Body Temp", $"{vitals.BodyTemperatureCelsius.ToString("0.0", CultureInfo.InvariantCulture)} C");
    }

    private void AddRow(string name, string value)
    {
        var row = new Label
        {
            Text = $"{name}: {value}",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        row.AddThemeFontSizeOverride("font_size", RowFontSize);
        row.AddThemeColorOverride("font_color", new Color(0.77f, 0.83f, 0.78f));
        AddChild(row);
    }

    private void ClearRows()
    {
        foreach (var child in GetChildren())
        {
            RemoveChild(child);
            child.QueueFree();
        }
    }

    private static string FormatMeter(BoundedMeter meter)
    {
        return $"{meter.Current} / {meter.Maximum}";
    }
}
