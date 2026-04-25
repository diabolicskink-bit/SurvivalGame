using Godot;
using SurvivalGame.Domain;

public partial class NpcLayer : Node2D
{
    private int _cellSize = 32;
    private NpcRoster? _npcs;

    public void Configure(NpcRoster npcs, int cellSize)
    {
        _npcs = npcs;
        _cellSize = cellSize;
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (_npcs is null)
        {
            return;
        }

        foreach (var npc in _npcs.AllNpcs)
        {
            DrawNpcMarker(npc);
        }
    }

    private void DrawNpcMarker(NpcState npc)
    {
        var center = CellToBoardPosition(npc.Position);
        var radius = Mathf.Max(5.0f, _cellSize * 0.26f);
        var outerColor = npc.IsDisabled
            ? new Color(0.28f, 0.28f, 0.25f)
            : new Color(0.78f, 0.34f, 0.22f);
        var innerColor = npc.IsDisabled
            ? new Color(0.46f, 0.45f, 0.39f)
            : new Color(0.96f, 0.68f, 0.38f);

        DrawCircle(center + new Vector2(2, 3), radius, new Color(0.01f, 0.012f, 0.01f, 0.45f));
        DrawCircle(center, radius, outerColor);
        DrawCircle(center, radius * 0.58f, innerColor);
        DrawLine(center + new Vector2(-radius * 0.55f, 0), center + new Vector2(radius * 0.55f, 0), new Color(0.22f, 0.09f, 0.06f), 1.5f);
        DrawLine(center + new Vector2(0, -radius * 0.55f), center + new Vector2(0, radius * 0.55f), new Color(0.22f, 0.09f, 0.06f), 1.5f);

        DrawHealthBar(center, npc.Health);
    }

    private void DrawHealthBar(Vector2 center, BoundedMeter health)
    {
        var width = Mathf.Max(12.0f, _cellSize * 0.62f);
        var height = Mathf.Max(2.0f, _cellSize * 0.09f);
        var topLeft = center + new Vector2(-width / 2.0f, _cellSize * 0.30f);
        var backgroundRect = new Rect2(topLeft, new Vector2(width, height));
        var fillRect = new Rect2(topLeft, new Vector2(width * health.Normalized, height));

        DrawRect(backgroundRect, new Color(0.08f, 0.035f, 0.03f), true);
        DrawRect(fillRect, new Color(0.76f, 0.12f, 0.1f), true);
    }

    private Vector2 CellToBoardPosition(GridPosition cell)
    {
        return new Vector2(
            (cell.X + 0.5f) * _cellSize,
            (cell.Y + 0.5f) * _cellSize
        );
    }
}
