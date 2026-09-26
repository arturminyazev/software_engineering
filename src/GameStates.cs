namespace Blackjack;

public enum GameState
{
    WaitingForDeal,
    PlayerTurn,
    DealerTurn,
    RoundFinished
}

// Паттерн «Состояние»: каждый класс определяет доступные действия на своём этапе.
// Перечисление выше — только название этапа для интерфейса.
internal abstract class RoundState
{
    public abstract GameState Kind { get; }
    public virtual bool CanStartRound { get { return false; } }
    public virtual bool CanHit { get { return false; } }
    public virtual bool CanStand { get { return false; } }
    public virtual void Enter(BlackjackGame game) { }
    public virtual void StartRound(BlackjackGame game) { }
    public virtual void Hit(BlackjackGame game) { }
    public virtual void Stand(BlackjackGame game) { }
}

internal class WaitingState : RoundState
{
    public override GameState Kind { get { return GameState.WaitingForDeal; } }
    public override bool CanStartRound { get { return true; } }
    public override void StartRound(BlackjackGame game) { game.DealRound(); }
}

internal class PlayerTurnState : RoundState
{
    public override GameState Kind { get { return GameState.PlayerTurn; } }
    public override bool CanHit { get { return true; } }
    public override bool CanStand { get { return true; } }
    public override void Hit(BlackjackGame game) { game.TakePlayerCard(); }
    public override void Stand(BlackjackGame game) { game.BeginDealerTurn(); }
}

internal class DealerTurnState : RoundState
{
    public override GameState Kind { get { return GameState.DealerTurn; } }

    public override void Enter(BlackjackGame game)
    {
        game.RevealDealerCards();
        // В нашей версии дилер берёт карту и на 17; останавливается начиная с 18.
        while (game.DealerHand.Score <= 17)
            game.TakeDealerCard();
        game.CompareHands();
    }
}

internal class FinishedState : RoundState
{
    public override GameState Kind { get { return GameState.RoundFinished; } }
    public override bool CanStartRound { get { return true; } }
    public override void StartRound(BlackjackGame game) { game.DealRound(); }
}
