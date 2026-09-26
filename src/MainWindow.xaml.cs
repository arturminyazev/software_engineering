using System.Runtime.InteropServices;
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
            StandButton.Focus();
        else if (Keyboard.FocusedElement == StandButton && !StandButton.IsEnabled)
            NewRoundButton.Focus();
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
            "Это демонстрация интерфейса, а не готовая игра. Карты и очки заранее заданы.\n\n" +
            "Ctrl+N — новая раздача.\nCtrl+H — взять демонстрационную карту.\n" +
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

