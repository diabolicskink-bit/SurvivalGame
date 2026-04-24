namespace SurvivalGame.Domain;

public sealed class TurnState
{
    public TurnState(int currentTurn = 0)
    {
        if (currentTurn < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(currentTurn), "Turn count cannot be negative.");
        }

        CurrentTurn = currentTurn;
    }

    public int CurrentTurn { get; private set; }

    public void Advance()
    {
        CurrentTurn++;
    }
}
