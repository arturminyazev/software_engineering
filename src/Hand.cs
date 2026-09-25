namespace Blackjack;

// Один и тот же класс используется для руки игрока и руки дилера.
public class Hand
{
    public List<Card> Cards { get; } = new List<Card>();

    public int Count
    {
        get { return Cards.Count; }
    }

    // Подсчёт очков, перебор и выигрышные сочетания добавим в лабораторной 4.

    public void AddCard(Card card)
    {
        ArgumentNullException.ThrowIfNull(card);
        Cards.Add(card);
    }

    public void Clear()
    {
        Cards.Clear();
    }
}

