namespace Blackjack;

// Колода из 52 карт. Перемешивание добавим в лабораторной 4.
public class Deck
{
    private List<Card> cards = new List<Card>();

    public int Count
    {
        get { return cards.Count; }
    }

    public Deck()
    {
        foreach (Suit suit in Enum.GetValues<Suit>())
        {
            foreach (Rank rank in Enum.GetValues<Rank>())
                cards.Add(new Card(suit, rank));
        }
    }

    // Берём первую карту и удаляем её из колоды.
    public Card DrawCard()
    {
        if (cards.Count == 0)
            throw new InvalidOperationException("В колоде больше нет карт.");

        Card card = cards[0];
        cards.RemoveAt(0);
        return card;
    }
}

