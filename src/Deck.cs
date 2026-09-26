namespace Blackjack;

// Колода хранит оставшиеся карты и умеет перемешиваться.
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

    // Для проверок можно создать колоду с известным порядком карт.
    public Deck(IEnumerable<Card> orderedCards)
    {
        ArgumentNullException.ThrowIfNull(orderedCards);
        cards = new List<Card>(orderedCards);
        if (cards.Any(card => card == null))
            throw new ArgumentException("В колоде не должно быть пустых карт.", nameof(orderedCards));
    }

    public void Shuffle()
    {
        // Фишер — Йетс: меняем каждую карту со случайной картой из ещё не обработанной части.
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int j = Random.Shared.Next(i + 1);
            Card temporary = cards[i];
            cards[i] = cards[j];
            cards[j] = temporary;
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

