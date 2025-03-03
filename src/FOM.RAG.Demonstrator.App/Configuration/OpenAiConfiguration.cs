namespace FOM.RAG.Demonstrator.App.Configuration;

public class OpenAiConfiguration
{
    public required string ApiKey { get; set; }

    public string EmbeddingModel { get; set; } = "text-embedding-3-small";

    public string ChatModel { get; set; } = "gpt-4o";
}
