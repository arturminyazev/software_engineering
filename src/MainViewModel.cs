using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Blackjack;

// Данные и действия окна. Здесь только заранее заданный пример для лабораторной 3.
public class MainViewModel : INotifyPropertyChanged
{
    private BlackjackGame game = new BlackjackGame();
    private bool demoRoundActive;
    private bool extraCardAdded;

    public string PlayerText { get; private set; } = "Карты ещё не выданы.\nОчки: —. Карт: 0.";
    public string DealerText { get; private set; } = "Карты ещё не выданы.\nОчки: —. Карт: 0.";
    public string StatusText { get; private set; } = "Начните демонстрационную раздачу.";
    public string ResultText { get; private set; } = "Раунд ещё не завершён.";
    public string SummaryText { get; private set; } = "Здесь появится последнее сообщение. Сводка — Ctrl+I.";

    // ObservableCollection сама сообщает WPF о добавлении новых строк.
    public ObservableCollection<string> History { get; } = new ObservableCollection<string>();

    public RelayCommand NewRoundCommand { get; }
    public RelayCommand HitCommand { get; }
    public RelayCommand StandCommand { get; }
    public RelayCommand SummaryCommand { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    // Окно передаст сообщение скринридеру через UI Automation.
    public event Action<string>? AnnouncementRequested;

    public MainViewModel()
    {
        NewRoundCommand = new RelayCommand(StartDemo);
        HitCommand = new RelayCommand(AddDemoCard, () => demoRoundActive && !extraCardAdded);
        StandCommand = new RelayCommand(FinishDemo, () => demoRoundActive);
        SummaryCommand = new RelayCommand(ShowSummary);
        History.Add("Учебный пример интерфейса. Нажмите «Новая раздача» или Ctrl+N.");
    }

    private void StartDemo()
    {
        game = new BlackjackGame();
        game.PlayerHand.AddCard(new Card(Suit.Clubs, Rank.Queen));
        game.PlayerHand.AddCard(new Card(Suit.Hearts, Rank.Six));
        game.DealerHand.AddCard(new Card(Suit.Spades, Rank.Nine));
        game.DealerHand.AddCard(new Card(Suit.Hearts, Rank.King));
        demoRoundActive = true;
        extraCardAdded = false;

        // Очки в этой лабораторной — готовые подписи, а не алгоритм подсчёта.
        PlayerText = $"Дама треф. Шестёрка червей.\nОчки: 16. Карт: {game.PlayerHand.Count}.";
        DealerText = $"Открытая карта: девятка пик. Вторая карта закрыта.\nСумма скрыта. Карт: {game.DealerHand.Count}.";
        StatusText = "Ход игрока. В примере можно взять одну дополнительную карту или остановиться.";
        ResultText = "Раунд ещё не завершён.";
        History.Add("Новая демонстрационная раздача.");
        History.Add("Игрок получил даму треф и шестёрку червей. Очки: 16. Карт: 2.");
        History.Add("Дилер получил девятку пик и закрытую карту. Карт: 2.");
        UpdateDisplay();
        Announce("Раздача началась. У вас 16 очков, 2 карты. У дилера открыта девятка пик.");
    }

    private void AddDemoCard()
    {
        game.PlayerHand.AddCard(new Card(Suit.Diamonds, Rank.Three));
        extraCardAdded = true;
        PlayerText = $"Дама треф. Шестёрка червей. Тройка бубен.\nОчки: 19. Карт: {game.PlayerHand.Count}.";
        StatusText = "Дополнительная карта показана. Нажмите «Остановиться», чтобы увидеть результат примера.";
        History.Add("Игрок взял тройку бубен. Очки: 19. Карт: 3.");
        UpdateDisplay();
        Announce("Тройка бубен. У вас 19 очков, 3 карты. В этом примере дополнительная карта только одна.");
    }

    private void FinishDemo()
    {
        demoRoundActive = false;
        DealerText = "Девятка пик. Король червей.\nОчки: 19. Карт: 2.";
        StatusText = "Демонстрационный раунд завершён. Можно начать новую раздачу.";

        // Два готовых исхода макета. Сравнение рук и правила появятся в лабораторной 4.
        if (extraCardAdded)
            ResultText = "Ничья. У вас 19 очков. У дилера 19 очков.";
        else
            ResultText = "Проигрыш. У вас 16 очков. У дилера 19 очков.";

        History.Add("Игрок остановился. Дилер раскрыл короля червей. Очки дилера: 19.");
        History.Add(ResultText);
        UpdateDisplay();
        Announce(ResultText);
    }

    private void ShowSummary()
    {
        if (game.PlayerHand.Count == 0)
        {
            Announce("Раздача ещё не началась. Нажмите Ctrl+N.");
            return;
        }

        string playerScore = extraCardAdded ? "19" : "16";
        string message = $"У вас {playerScore} очков. Карт: {game.PlayerHand.Count}. ";
        if (demoRoundActive)
            message += "У дилера открыта девятка пик. Вторая карта закрыта.";
        else
            message += $"{ResultText} Раунд завершён.";

        Announce(message);
    }

    private void Announce(string message)
    {
        SummaryText = message;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SummaryText)));
        // Событие отправляется даже при повторной сводке с тем же текстом.
        AnnouncementRequested?.Invoke(message);
    }

    private void UpdateDisplay()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlayerText)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DealerText)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusText)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ResultText)));
        HitCommand.UpdateCanExecute();
        StandCommand.UpdateCanExecute();
    }
}

