using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Blackjack.Tests;

[TestClass]
public class HandTests
{
    [TestMethod]
    public void AddCard_TwoAces_ProducesSpecialHandWithoutBust()
    {
        // Подготовка.
        Hand hand = new Hand();

        // Действие.
        hand.AddCard(new Card(Suit.Spades, Rank.Ace));
        hand.AddCard(new Card(Suit.Hearts, Rank.Ace));

        // Проверка: исключение действует ровно для двух тузов.
        Assert.AreEqual(22, hand.Score);
        Assert.IsTrue(hand.HasTwoAces);
        Assert.IsFalse(hand.IsBust);
    }

    [TestMethod]
    public void AddCard_SixAfterTwoAces_Produces28AndBust()
    {
        // Подготовка: особое сочетание из двух тузов.
        Hand hand = new Hand();
        hand.AddCard(new Card(Suit.Spades, Rank.Ace));
        hand.AddCard(new Card(Suit.Hearts, Rank.Ace));

        // Действие: добавить третью карту непосредственно в модель руки.
        hand.AddCard(new Card(Suit.Clubs, Rank.Six));

        // Проверка: с третьей картой исключение больше не действует.
        Assert.AreEqual(28, hand.Score);
        Assert.IsFalse(hand.HasTwoAces);
        Assert.IsTrue(hand.IsBust);
    }
}

