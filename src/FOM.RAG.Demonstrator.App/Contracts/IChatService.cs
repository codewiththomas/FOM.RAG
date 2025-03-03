using OpenAI.Chat;

namespace FOM.RAG.Demonstrator.App.Contracts;

public interface IChatService
{
    Task<string> GetCompletionAsync(string userMessage, List<ChatMessage> history);
    IAsyncEnumerable<string> StreamCompletionAsync(string userMessage, List<ChatMessage> history);
}
