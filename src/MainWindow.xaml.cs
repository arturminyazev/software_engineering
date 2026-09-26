using System.Runtime.InteropServices;
using System.Media;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;

namespace Blackjack;

public partial class MainWindow : Window
{
    private readonly MainViewModel viewModel = new MainViewModel();

    public MainWindow()
    {
        InitializeComponent();
        // Все {Binding ...} в окне получают данные из этого объекта.
        DataContext = viewModel;
        viewModel.AnnouncementRequested += AnnounceToScreenReader;
        viewModel.ResultSoundRequested += PlayResultSound;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        NewRoundButton.Focus();
    }

    private void AnnounceToScreenReader(string message)
    {
        // Это работа интерфейса, а не правила игры. Собственный голос не запускается.
        AutomationPeer? peer = UIElementAutomationPeer.FromElement(SummaryBox)
            ?? UIElementAutomationPeer.CreatePeerForElement(SummaryBox);
        peer?.RaiseNotificationEvent(AutomationNotificationKind.Other,
            AutomationNotificationProcessing.ImportantMostRecent, message, "BlackjackFeedback");

        // Если нажатая кнопка стала недоступна, оставляем фокус на доступном действии.
        if (Keyboard.FocusedElement == HitButton && !HitButton.IsEnabled)
        {
            if (StandButton.IsEnabled)
                StandButton.Focus();
            else
                NewRoundButton.Focus();
        }
        else if (Keyboard.FocusedElement == StandButton && !StandButton.IsEnabled)
            NewRoundButton.Focus();
        else if (Keyboard.FocusedElement == NewRoundButton && !NewRoundButton.IsEnabled)
            HitButton.Focus();
    }

    private void PlayResultSound(RoundOutcome outcome)
    {
        // Звук дополняет текст; точный результат всегда доступен скринридеру.
        if (outcome == RoundOutcome.Win)
            SystemSounds.Asterisk.Play();
        else if (outcome == RoundOutcome.Loss)
            SystemSounds.Hand.Play();
        else
            SystemSounds.Beep.Play();
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.L && Keyboard.Modifiers == ModifierKeys.Control)
        {
            FocusHistory();
            e.Handled = true;
        }
        else if (e.Key == Key.F1 && Keyboard.Modifiers == ModifierKeys.None)
        {
            ShowHelp();
            e.Handled = true;
        }
    }

    private void FocusHistory_Click(object sender, RoutedEventArgs e)
    {
        FocusHistory();
    }

    private void FocusHistory()
    {
        if (HistoryList.SelectedIndex < 0 && HistoryList.Items.Count > 0)
            HistoryList.SelectedIndex = 0;

        HistoryList.Focus();
        if (HistoryList.SelectedItem != null)
        {
            HistoryList.ScrollIntoView(HistoryList.SelectedItem);
            HistoryList.UpdateLayout();
            if (HistoryList.ItemContainerGenerator.ContainerFromItem(HistoryList.SelectedItem) is ListBoxItem item)
                item.Focus();
        }
    }

    private void HistoryList_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control
            && HistoryList.SelectedItem is string message)
        {
            try
            {
                Clipboard.SetText(message);
            }
            catch (ExternalException)
            {
                MessageBox.Show(this, "Буфер обмена занят. Попробуйте скопировать сообщение ещё раз.",
                    "Копирование", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            e.Handled = true;
        }
    }

    private void Help_Click(object sender, RoutedEventArgs e)
    {
        ShowHelp();
    }

    private void ShowHelp()
    {
        MessageBox.Show(this,
            "Туз всегда стоит 11, картинки — 10, остальные карты — по номиналу.\n" +
            "Ровно два туза сильнее блэкджека. Блэкджек — 21 на первых двух картах.\n" +
            "Одинаковые особые сочетания дают ничью. С тремя картами исключение двух тузов не действует.\n" +
            "Дилер берёт карту при 17 и меньше, останавливается на 18–21.\n" +
            "При 21 очке ваш ход завершается автоматически. Перебор завершает раунд проигрышем.\n\n" +
            "Ctrl+N — новая раздача, доступна после завершения текущего раунда.\nCtrl+H — взять карту.\n" +
            "Ctrl+J — остановиться.\nCtrl+I — повторить сводку без переноса фокуса.\n" +
            "Ctrl+L — перейти в историю.\nF1 — эта справка.\n\n" +
            "Tab и Shift+Tab — переход по элементам. Enter или пробел — нажать кнопку.\n" +
            "В истории: стрелки — сообщения, Home и End — начало и конец, Ctrl+C — копировать выбранное.\n" +
            "Сведения о картах и результате можно читать и копировать как обычный текст.\n" +
            "Новые сообщения не перемещают выбранную строку истории.\n\n" +
            "Сообщения передаются скринридеру через UI Automation. Если они не озвучиваются, " +
            "прочитайте поле «Последнее сообщение и сводка».\n\n" +
            "Esc — закрыть справку. Alt+F4 — выйти из приложения.",
            "Справка по управлению", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}

