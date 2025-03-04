using FOM.RAG.Demonstrator.App.Configuration;
using FOM.RAG.Demonstrator.App.Contracts;
using FOM.RAG.Demonstrator.App.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using OpenAI.Embeddings;
using System.Text;

namespace FOM.RAG.Demonstrator.App.Services;

public class RagChatService : IChatService
{
    private readonly InMemoryVectorStore _vectorStore;
    private readonly OpenAiConfiguration _openAiConfig;
    private readonly ILogger _logger;

    public RagChatService(
        InMemoryVectorStore vectorStore,
        ILogger<RagChatService> logger,
        IOptions<OpenAiConfiguration> openAiConfiguration)
    {
        _vectorStore = vectorStore;
        _openAiConfig = openAiConfiguration.Value;
        _logger = logger;
    }

    public async Task<string> GetCompletionAsync(string userMessage, List<ChatMessage> history)
    {
        var chatContext = await BuildRagContext(userMessage);

        var messages = new List<ChatMessage>(history);

        // Ensure the last message is our RAG-enhanced message
        if (messages.Count > 0 && messages[^1] is UserChatMessage)
        {
            messages.RemoveAt(messages.Count - 1);
        }

        // Add the system message if it doesn't exist
        if (messages.Count == 0 || messages[0] is not SystemChatMessage)
        {
            messages.Insert(0, new SystemChatMessage(
                @"You are a helpful assistant for a knowledge base. Answer the user's question based on the context provided. If the answer cannot be found in the context, say that you don't know the answer. Be concise and accurate in your answers. Answer in German only."));
        }

        // Add the RAG-enhanced user message
        messages.Add(new UserChatMessage($"{chatContext}\n\nBased on the information above, please answer this question in German: {userMessage}"));

        var chatClient = new ChatClient(_openAiConfig.ChatModel, _openAiConfig.ApiKey);
        var response = await chatClient.CompleteChatAsync(messages);

        return response.Value.Content[0].Text;
    }

    public async IAsyncEnumerable<string> StreamCompletionAsync(string userMessage, List<ChatMessage> history)
    {
        var chatContext = await BuildRagContext(userMessage, 5);
        _logger.LogInformation("Context generated: {context}", chatContext);

        var messages = new List<ChatMessage>();

        // Add previous conversation history, excluding the last user message
        foreach (var msg in history.Take(history.Count - 1))
        {
            messages.Add(msg);
        }

        // Add the system message if it doesn't exist
        if (messages.Count == 0 || messages[0] is not SystemChatMessage)
        {
            messages.Insert(0, new SystemChatMessage(
                @"You are a helpful assistant for a knowledge base. Answer the user's question based on the context provided. If the answer cannot be found in the context, say that you don't know the answer. Be concise and accurate in your answers. Answer in German only."));
        }

        // Add the RAG-enhanced user message
        messages.Add(new UserChatMessage($"{chatContext}\n\nBased on the information above, please answer this question in German: {userMessage}"));

        var chatClient = new ChatClient(_openAiConfig.ChatModel, _openAiConfig.ApiKey);

        _logger.LogInformation("Starting chat completion stream");
        var completionUpdates = chatClient.CompleteChatStreamingAsync(messages);

        await foreach (var update in completionUpdates)
        {
            _logger.LogInformation("Received completion update: {update}", update);
            if (update.ContentUpdate.Count > 0 && !string.IsNullOrEmpty(update.ContentUpdate[0].Text))
            {
                yield return update.ContentUpdate[0].Text;
            }
        }
    }

    private async Task<string> BuildRagContext(string question, int maxContextChunks = 5)
    {
        // Get embeddings for the question
        var embeddingClient = new EmbeddingClient(_openAiConfig.EmbeddingModel, _openAiConfig.ApiKey);

        _logger.LogInformation("Generating embedding for question: {question}", question);
        OpenAIEmbedding questionEmbedding = embeddingClient.GenerateEmbedding(question);
        _logger.LogInformation("Embedding generated: {embedding}", 
            string.Join(",", questionEmbedding.ToFloats().ToArray().Select(x => x.ToString())));

        // Find similar chunks in the vector store
        _logger.LogInformation("Finding similar embeddings in vector store");
        var similarEmbeddings = _vectorStore.FindSimilar(questionEmbedding.ToFloats(), maxContextChunks);
        _logger.LogInformation("Results found: {i}", similarEmbeddings.Count);
        
        if (similarEmbeddings.Count == 0)
        {
            return "CONTEXT INFORMATION:\nEs konnten keine relevanten Informationen zur Frage gefunden werden.";
        }

        // Build context string from similar chunks
        var contextBuilder = new StringBuilder();
        contextBuilder.AppendLine("CONTEXT INFORMATION:");

        foreach (var (chunk, score) in similarEmbeddings)
        {
            contextBuilder.AppendLine("---");
            contextBuilder.AppendLine($"Relevance Score: {score:F2}");
            contextBuilder.AppendLine(chunk.Content);
            contextBuilder.AppendLine("---");
        }

        return contextBuilder.ToString();
    }
}
