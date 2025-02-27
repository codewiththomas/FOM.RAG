namespace FOM.RAG.Demonstrator.Configuration;

public class DemonstratorConfiguration
{
    public required string BaseFolder { get; set; }
    public bool IsCrawlingActivated { get; set; }
    public bool IsCleaningActivated { get; set; }
    public bool IsEmbeddingActivated { get; set; }
}