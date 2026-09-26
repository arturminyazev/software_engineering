namespace Blackjack;

// Список этапов находится в том же файле, что и игра.
public enum GameState
{
    WaitingForDeal,
    PlayerTurn,
    DealerTurn,
    RoundFinished
}

// Пока игра только объединяет данные. Методы проведения раунда добавим позже.
public class BlackjackGame
{
    public Deck Deck { get; } = new Deck();
    public Hand PlayerHand { get; } = new Hand();
    public Hand DealerHand { get; } = new Hand();

    // История — обычный список текстовых сообщений.
    public List<string> History { get; } = new List<string>();

    // Пока раунд не начат, результата нет.
    public GameState State { get; private set; } = GameState.WaitingForDeal;
    public string ResultMessage { get; private set; } = "";
}

