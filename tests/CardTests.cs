using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Blackjack.Tests;

[TestClass]
public class CardTests
{
    [TestMethod]
    public void BaseValue_AllRanks_ReturnExpectedPoints()
    {
        // Подготовка: ожидаемые значения от туза до короля.
        int[] expected = { 11, 2, 3, 4, 5, 6, 7, 8, 9, 10, 10, 10, 10 };

        for (int rank = 1; rank <= 13; rank++)
        {
            Card card = new Card(Suit.Spades, (Rank)rank);

            // Действие: запросить стоимость карты.
            int actual = card.BaseValue;

            // Проверка: стоимость должна соответствовать правилу.
            Assert.AreEqual(expected[rank - 1], actual, $"Неверная стоимость: {card.Name}");
        }
    }
}
