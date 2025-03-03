using FOM.RAG.Demonstrator.App.Configuration;
using FOM.RAG.Demonstrator.App.Contracts;
using FOM.RAG.Demonstrator.App.Models;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using OpenAI.Embeddings;
using System.Text;

namespace FOM.RAG.Demonstrator.App.Services;

public class RagService(
    InMemoryVectorStore vectorStore,
    IChatService chatService,
    IOptions<OpenAiConfiguration> openAiConfiguration)
{
    public async Task<string> AnswerQuestion(string question, int maxContextChunks = 5)
    {
        var embeddingClient = new EmbeddingClient(openAiConfiguration.Value.EmbeddingModel, openAiConfiguration.Value.ApiKey);
        OpenAIEmbedding questionEmbedding = embeddingClient.GenerateEmbedding(question);

        var similarEmbeddings = vectorStore.FindSimilar(questionEmbedding.ToFloats(), maxContextChunks);

        if (similarEmbeddings.Count == 0)
        {
            return "Es konnten keine Embeddings zur Frage gefunden werden.";
        }

        var contextBuilder = new StringBuilder();
        contextBuilder.AppendLine("CONTEXT INFORMATION:");

        foreach (var (chunk, score) in similarEmbeddings)
        {
            contextBuilder.AppendLine("---");
            contextBuilder.AppendLine($"Relevance Score: {score:F2}");
            contextBuilder.AppendLine(chunk.Content);
            contextBuilder.AppendLine("---");
        }

        var chatClient = new ChatClient(openAiConfiguration.Value.ChatModel, openAiConfiguration.Value.ApiKey);

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(
                @"You are a helpful assistant for a knowledge base. Answer the user's question based on the context provided. If the answer cannot be found in the context, say that you don't know the answer. Be concise and accurate in your answers. Answer in German only."),
            new UserChatMessage($"{contextBuilder}\n\nBased on the information above, please answer this question in German: {question}")
        };

        var response = await chatClient.CompleteChatAsync(messages);

        var answer = response.Value.Content[0].Text;

        return answer;
    }
}