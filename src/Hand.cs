namespace Blackjack;

// Один и тот же класс используется для руки игрока и руки дилера.
public class Hand
{
    public List<Card> Cards { get; } = new List<Card>();

    public int Count
    {
        get { return Cards.Count; }
    }

    public int Score
    {
        get
        {
            int total = 0;
            foreach (Card card in Cards)
                total += card.BaseValue;
            return total;
        }
    }

    public bool HasTwoAces
    {
        get { return Count == 2 && Cards[0].Rank == Rank.Ace && Cards[1].Rank == Rank.Ace; }
    }

    public bool IsBlackjack
    {
        get { return Count == 2 && Score == 21; }
    }

    public bool IsBust
    {
        // Два туза дают 22, но по нашим правилам это особое выигрышное сочетание.
        get { return Score > 21 && !HasTwoAces; }
    }

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

