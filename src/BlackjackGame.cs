namespace Blackjack;

// Исход раунда и этап игры — разные понятия.
public enum RoundOutcome
{
    Win,
    Loss,
    Draw
}

// Игровые правила не зависят от WPF, кнопок или озвучивания.
public class BlackjackGame
{
    private RoundState currentState = new WaitingState();
    private readonly Func<Deck> createDeck;
    private bool actionInProgress;

    public Deck Deck { get; private set; } = new Deck();
    public Hand PlayerHand { get; } = new Hand();
    public Hand DealerHand { get; } = new Hand();
    public List<string> History { get; } = new List<string>();
    public GameState State { get { return currentState.Kind; } }
    public RoundOutcome? Outcome { get; private set; }
    public string ResultMessage { get; private set; } = "";
    public bool DealerCardsRevealed { get; private set; }
    public int RoundNumber { get; private set; }

    public bool CanStartRound { get { return !actionInProgress && currentState.CanStartRound; } }
    public bool CanHit { get { return !actionInProgress && currentState.CanHit; } }
    public bool CanStand { get { return !actionInProgress && currentState.CanStand; } }

    // Наблюдатель: подписчики получают события, но игра ничего не знает об окне.
    public event Action? Changed;
    public event Action<string>? MessageAdded;
    public event Action<RoundOutcome>? RoundEnded;

    public BlackjackGame() : this(CreateShuffledDeck)
    {
    }

    // Дополнительный конструктор нужен для воспроизводимых проверок без случайности.
    public BlackjackGame(Func<Deck> createDeck)
    {
        ArgumentNullException.ThrowIfNull(createDeck);
        this.createDeck = createDeck;
    }

    private static Deck CreateShuffledDeck()
    {
        Deck deck = new Deck();
        deck.Shuffle();
        return deck;
    }

    public void StartNewRound()
    {
        if (CanStartRound)
            RunAction(() => currentState.StartRound(this));
    }

    public void Hit()
    {
        if (CanHit)
            RunAction(() => currentState.Hit(this));
    }

    public void Stand()
    {
        if (CanStand)
            RunAction(() => currentState.Stand(this));
    }

    private void RunAction(Action action)
    {
        // Не допускаем повторный ход внутри обработки событий текущего действия.
        actionInProgress = true;
        try
        {
            action();
        }
        finally
        {
            actionInProgress = false;
            Changed?.Invoke();
        }
    }

    internal void ChangeState(RoundState state)
    {
        currentState = state;
        currentState.Enter(this);
    }

    internal void DealRound()
    {
        Deck newDeck = createDeck();
        if (newDeck == null || newDeck.Count < 4)
            throw new InvalidOperationException("Для новой раздачи нужны хотя бы четыре карты.");

        Deck = newDeck;
        PlayerHand.Clear();
        DealerHand.Clear();
        DealerCardsRevealed = false;
        Outcome = null;
        ResultMessage = "";
        RoundNumber++;
        AddMessage($"Новый раунд № {RoundNumber}.");

        // Порядок выдачи: игрок, дилер, игрок, дилер.
        GivePlayerCard();
        Card openCard = Deck.DrawCard();
        DealerHand.AddCard(openCard);
        AddMessage($"Открытая карта дилера: {openCard.Name}. Карт: 1.");
        GivePlayerCard();
        DealerHand.AddCard(Deck.DrawCard());
        AddMessage("Дилер получил закрытую карту. Карт: 2. Полная сумма скрыта.");

        // Сначала сравниваем особые стартовые сочетания, и только потом обычные суммы.
        int playerPriority = GetStartingPriority(PlayerHand);
        int dealerPriority = GetStartingPriority(DealerHand);
        if (playerPriority > 0 || dealerPriority > 0)
        {
            if (playerPriority == dealerPriority)
                FinishRound(RoundOutcome.Draw, "Одинаковые особые стартовые сочетания.");
            else if (playerPriority > dealerPriority)
                FinishRound(RoundOutcome.Win, "Ваше стартовое сочетание сильнее.");
            else
                FinishRound(RoundOutcome.Loss, "Стартовое сочетание дилера сильнее.");
            return;
        }

        ChangeState(new PlayerTurnState());
        AddMessage("Ваш ход. Можно взять карту или остановиться.");
    }

    private static int GetStartingPriority(Hand hand)
    {
        if (hand.HasTwoAces)
            return 2;
        if (hand.IsBlackjack)
            return 1;
        return 0;
    }

    private void GivePlayerCard()
    {
        Card card = Deck.DrawCard();
        PlayerHand.AddCard(card);
        AddMessage($"Игрок получил карту: {card.Name}. Очки: {PlayerHand.Score}. Карт: {PlayerHand.Count}.");
    }

    internal void TakePlayerCard()
    {
        GivePlayerCard();
        if (PlayerHand.IsBust)
            FinishRound(RoundOutcome.Loss, "У вас перебор.");
        else if (PlayerHand.Score == 21)
        {
            AddMessage("У вас 21 очко. Ход автоматически переходит к дилеру.");
            ChangeState(new DealerTurnState());
        }
    }

    internal void BeginDealerTurn()
    {
        AddMessage($"Игрок остановился. Очки: {PlayerHand.Score}.");
        ChangeState(new DealerTurnState());
    }

    internal void RevealDealerCards()
    {
        if (DealerCardsRevealed)
            return;
        DealerCardsRevealed = true;
        AddMessage($"Дилер раскрыл карту: {DealerHand.Cards[1].Name}. " +
            $"Очки дилера: {DealerHand.Score}. Карт: {DealerHand.Count}.");
    }

    internal void TakeDealerCard()
    {
        Card card = Deck.DrawCard();
        DealerHand.AddCard(card);
        AddMessage($"Дилер получил карту: {card.Name}. Очки: {DealerHand.Score}. Карт: {DealerHand.Count}.");
    }

    internal void CompareHands()
    {
        if (DealerHand.IsBust)
            FinishRound(RoundOutcome.Win, "У дилера перебор.");
        else if (PlayerHand.Score > DealerHand.Score)
            FinishRound(RoundOutcome.Win, "Ваша сумма больше.");
        else if (PlayerHand.Score < DealerHand.Score)
            FinishRound(RoundOutcome.Loss, "Сумма дилера больше.");
        else
            FinishRound(RoundOutcome.Draw, "Суммы очков равны.");
    }

    private void FinishRound(RoundOutcome outcome, string reason)
    {
        RevealDealerCards();
        Outcome = outcome;
        string result = outcome switch
        {
            RoundOutcome.Win => "Победа", RoundOutcome.Loss => "Проигрыш", _ => "Ничья"
        };
        string specialHands = "";
        if (PlayerHand.HasTwoAces)
            specialHands += " У вас два туза — особое сочетание, не перебор.";
        else if (PlayerHand.IsBlackjack)
            specialHands += " У вас блэкджек.";
        if (DealerHand.HasTwoAces)
            specialHands += " У дилера два туза — особое сочетание, не перебор.";
        else if (DealerHand.IsBlackjack)
            specialHands += " У дилера блэкджек.";

        ResultMessage = $"{result}. {reason} У вас {PlayerHand.Score} очков. " +
            $"У дилера {DealerHand.Score} очков.{specialHands}";
        ChangeState(new FinishedState());
        AddMessage(ResultMessage);
        RoundEnded?.Invoke(outcome);
    }

    private void AddMessage(string message)
    {
        History.Add(message);
        MessageAdded?.Invoke(message);
    }

    public string GetSummary()
    {
        if (PlayerHand.Count == 0)
            return "Раздача ещё не началась. Нажмите Ctrl+N.";
        string message = $"У вас {PlayerHand.Score} очков. Карт: {PlayerHand.Count}. ";
        if (DealerHand.Count == 0)
            return message + "Идёт раздача карт.";
        if (!DealerCardsRevealed)
            return message + $"Открытая карта дилера: {DealerHand.Cards[0].Name}. Вторая карта закрыта.";
        if (State != GameState.RoundFinished)
            return message + $"У дилера {DealerHand.Score} очков. Ход дилера.";
        return message + ResultMessage + " Раунд завершён.";
    }
}

