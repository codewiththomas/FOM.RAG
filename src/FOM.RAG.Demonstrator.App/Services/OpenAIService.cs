using FOM.RAG.Demonstrator.App.Contracts;
using OpenAI.Chat;

namespace FOM.RAG.Demonstrator.App.Services;


public class OpenAIService : IChatService
{
    private readonly ChatClient _chatClient;
    private readonly string _model;

    public OpenAIService(IConfiguration configuration)
    {
        var apiKey = configuration["OpenAI:ApiKey"]
            ?? throw new ArgumentNullException("OpenAI API key is not configured");

        _model = configuration["OpenAI:Model"] ?? "gpt-4o";
        _chatClient = new ChatClient(_model, apiKey: apiKey);
    }

    public async Task<string> GetCompletionAsync(string userMessage, List<ChatMessage> history)
    {
        var messages = ConvertToChatMessages(history, userMessage);
        var completion = await _chatClient.CompleteChatAsync(messages);
        //return completion.Content[0].Text;
        return completion.Value.Content[0].Text;
    }

    public async IAsyncEnumerable<string> StreamCompletionAsync(string userMessage, List<ChatMessage> history)
    {
        var messages = ConvertToChatMessages(history, userMessage);

        var completionUpdates = _chatClient.CompleteChatStreamingAsync(messages);

        await foreach (var update in completionUpdates)
        {
            if (update.ContentUpdate.Count > 0 && !string.IsNullOrEmpty(update.ContentUpdate[0].Text))
            {
                yield return update.ContentUpdate[0].Text;
            }
        }
    }

    private List<ChatMessage> ConvertToChatMessages(List<ChatMessage> history, string userMessage)
    {
        // Convert our app's ChatMessage objects to OpenAI SDK ChatMessage objects
        var messages = new List<ChatMessage>();

        // Add system message
        messages.Add(new SystemChatMessage("You are a helpful assistant."));

        // Add conversation history
        foreach (var msg in history)
        {
            if (msg is UserChatMessage)
            {
                messages.Add(new UserChatMessage(msg.Content));
            }
            else if (msg is AssistantChatMessage)
            {
                messages.Add(new AssistantChatMessage(msg.Content));
            }
        }

        // Add current user message
        messages.Add(new UserChatMessage(userMessage));

        return messages;
    }
}