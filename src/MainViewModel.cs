using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;

namespace Blackjack;

// ViewModel готовит текст и связывает команды окна с настоящей игрой.
public class MainViewModel : INotifyPropertyChanged
{
    private readonly BlackjackGame game;
    private readonly HistorySaver historySaver = new HistorySaver();

    public string PlayerText { get { return DescribeHand(game.PlayerHand); } }
    public string DealerText
    {
        get
        {
            if (game.DealerHand.Count == 0)
                return "Карты ещё не выданы.\nОчки: —. Карт: 0.";
            if (game.DealerCardsRevealed)
                return DescribeHand(game.DealerHand);
            return $"Открытая карта: {game.DealerHand.Cards[0].Name}. Вторая карта закрыта.\n" +
                $"Сумма скрыта. Карт: {game.DealerHand.Count}.";
        }
    }
    public string StatusText
    {
        get
        {
            return game.State switch
            {
                GameState.WaitingForDeal => "Начните новую раздачу: Ctrl+N.",
                GameState.PlayerTurn => "Ваш ход. Взять карту — Ctrl+H. Остановиться — Ctrl+J.",
                GameState.DealerTurn => "Ход дилера.",
                _ => "Раунд завершён. Новая раздача — Ctrl+N."
            };
        }
    }
    public string ResultText
    {
        get { return game.ResultMessage == "" ? "Раунд ещё не завершён." : game.ResultMessage; }
    }
    public string SummaryText { get; private set; } = "Здесь появится последнее сообщение. Сводка — Ctrl+I.";
    public ObservableCollection<string> History { get; } = new ObservableCollection<string>();

    public RelayCommand NewRoundCommand { get; }
    public RelayCommand HitCommand { get; }
    public RelayCommand StandCommand { get; }
    public RelayCommand SummaryCommand { get; }
    public AsyncRelayCommand SaveHistoryCommand { get; }

    // Окно выбирает путь, ViewModel сохраняет сообщения. null означает отмену.
    public Func<string?>? ChooseHistoryPath { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event Action<string>? AnnouncementRequested;
    public event Action<RoundOutcome>? ResultSoundRequested;

    public MainViewModel() : this(new BlackjackGame())
    {
    }

    // В проверке можно передать игру с известной колодой.
    public MainViewModel(BlackjackGame game)
    {
        ArgumentNullException.ThrowIfNull(game);
        this.game = game;
        NewRoundCommand = new RelayCommand(() => ExecuteGameAction(game.StartNewRound), () => game.CanStartRound);
        HitCommand = new RelayCommand(() => ExecuteGameAction(game.Hit), () => game.CanHit);
        StandCommand = new RelayCommand(() => ExecuteGameAction(game.Stand), () => game.CanStand);
        SummaryCommand = new RelayCommand(() => Announce(game.GetSummary()));
        SaveHistoryCommand = new AsyncRelayCommand(SaveHistoryAsync, () => History.Count > 0);
        History.CollectionChanged += (_, _) => SaveHistoryCommand.UpdateCanExecute();

        foreach (string message in game.History)
            History.Add(message);

        // Наблюдатель: реагируем на модель; игровая логика не обращается к окну.
        game.Changed += UpdateDisplay;
        game.MessageAdded += History.Add;
        game.RoundEnded += outcome => ResultSoundRequested?.Invoke(outcome);
    }

    private async Task SaveHistoryAsync()
    {
        try
        {
            string? path = ChooseHistoryPath?.Invoke();
            if (path == null)
                return;

            // Сохраняем снимок: новые ходы не изменят уже начатую запись.
            string[] messages = History.ToArray();
            Announce("Сохраняется история сообщений.");
            await historySaver.SaveAsync(path, messages);
            Announce($"История сохранена. Файл: {path}");
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException
            or ArgumentException or NotSupportedException)
        {
            Announce($"Не удалось сохранить историю. {error.Message}");
        }
    }

    private static string DescribeHand(Hand hand)
    {
        if (hand.Count == 0)
            return "Карты ещё не выданы.\nОчки: —. Карт: 0.";
        string cards = string.Join(". ", hand.Cards.Select(card => card.Name));
        string text = $"{cards}.\nОчки: {hand.Score}. Карт: {hand.Count}.";
        if (hand.HasTwoAces)
            text += " Два туза — особое сочетание, не перебор.";
        else if (hand.IsBlackjack)
            text += " Блэкджек.";
        return text;
    }

    private void ExecuteGameAction(Action action)
    {
        int previousCount = game.History.Count;
        action();
        // Все события одного хода озвучиваются одним сообщением в правильном порядке.
        // Поэтому сообщения о картах дилера не вытесняют друг друга при быстром ходе.
        string message = string.Join(" ", game.History.Skip(previousCount));
        if (message != "")
            Announce(message);
    }

    private void Announce(string message)
    {
        SummaryText = message;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SummaryText)));
        // Повторная сводка отправляется даже при неизменившемся тексте.
        AnnouncementRequested?.Invoke(message);
    }

    private void UpdateDisplay()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlayerText)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DealerText)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusText)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ResultText)));
        NewRoundCommand.UpdateCanExecute();
        HitCommand.UpdateCanExecute();
        StandCommand.UpdateCanExecute();
    }
}
