namespace Blackjack;

// Масти и достоинства описаны здесь же, рядом с классом карты.
public enum Suit
{
    Clubs,
    Diamonds,
    Hearts,
    Spades
}

public enum Rank
{
    Ace = 1,
    Two = 2,
    Three = 3,
    Four = 4,
    Five = 5,
    Six = 6,
    Seven = 7,
    Eight = 8,
    Nine = 9,
    Ten = 10,
    Jack = 11,
    Queen = 12,
    King = 13
}

// Одна карта: масть, достоинство и базовая стоимость.
public class Card
{
    public Suit Suit { get; }
    public Rank Rank { get; }

    public int BaseValue
    {
        get
        {
            // В нашей версии туз всегда стоит 11 и не пересчитывается в 1.
            if (Rank == Rank.Ace)
                return 11;

            if (Rank == Rank.Jack || Rank == Rank.Queen || Rank == Rank.King)
                return 10;

            return (int)Rank;
        }
    }

    public Card(Suit suit, Rank rank)
    {
        if (!Enum.IsDefined(suit))
            throw new ArgumentOutOfRangeException(nameof(suit));
        if (!Enum.IsDefined(rank))
            throw new ArgumentOutOfRangeException(nameof(rank));

        Suit = suit;
        Rank = rank;
    }
}

