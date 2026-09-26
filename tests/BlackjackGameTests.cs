using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Blackjack.Tests;

[TestClass]
public class BlackjackGameTests
{
    [TestMethod]
    public void Stand_DealerHas17_DrawsAnotherCard()
    {
        // Подготовка: игрок 19, дилер 17, следующая карта — двойка.
        BlackjackGame game = CreateGame(Rank.Ten, Rank.Ten, Rank.Nine, Rank.Seven, Rank.Two);
        game.StartNewRound();

        // Действие: игрок прекращает добор.
        game.Stand();

        // Проверка: дилер взял двойку и остановился на 19.
        Assert.AreEqual(3, game.DealerHand.Count);
        Assert.AreEqual(Rank.Two, game.DealerHand.Cards[2].Rank);
        Assert.AreEqual(19, game.DealerHand.Score);
        Assert.AreEqual(47, game.Deck.Count);
        Assert.AreEqual(GameState.RoundFinished, game.State);
    }

    [TestMethod]
    public void Stand_DealerHas18_DoesNotDraw()
    {
        // Подготовка: игрок 19, дилер 18.
        BlackjackGame game = CreateGame(Rank.Ten, Rank.Ten, Rank.Nine, Rank.Eight);
        game.StartNewRound();

        // Действие.
        game.Stand();

        // Проверка: колода не расходуется во время хода дилера.
        Assert.AreEqual(2, game.DealerHand.Count);
        Assert.AreEqual(18, game.DealerHand.Score);
        Assert.AreEqual(48, game.Deck.Count);
        Assert.AreEqual(GameState.RoundFinished, game.State);
    }

    // Переставляем указанные карты в начало настоящей колоды из 52 карт.
    // Выдача идёт по очереди: игрок, дилер, игрок, дилер, затем добор.
    private static BlackjackGame CreateGame(params Rank[] firstRanks)
    {
        Deck original = new Deck();
        List<Card> remaining = new List<Card>();
        while (original.Count > 0)
            remaining.Add(original.DrawCard());

        List<Card> ordered = new List<Card>();
        foreach (Rank rank in firstRanks)
        {
            Card card = remaining.First(card => card.Rank == rank);
            remaining.Remove(card);
            ordered.Add(card);
        }
        ordered.AddRange(remaining);

        // При запросе колоды игра получает именно этот порядок, без перемешивания.
        return new BlackjackGame(() => new Deck(ordered));
    }
}

