using System.IO;
using System.Text;

namespace Blackjack;

// Запись истории в файл
public class HistorySaver
{
    // Вызывающий код передаёт путь и снимок сообщений в нужном порядке.
    // Ошибки передаются вызывающему коду, который сообщит о них пользователю.
    public async Task SaveAsync(string path, string[] messages)
    {
        await File.WriteAllLinesAsync(path, messages, Encoding.UTF8);
    }
}
