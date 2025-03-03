namespace FOM.RAG.Demonstrator.App.Models;

public class DocumentEmbedding
{
    public Guid DocumentId { get; set; }
    public Guid ChunkId { get; set; }

    public string Content { get; set; }

    public string FilePath { get; set; }

    public ReadOnlyMemory<float> Vector { get; set; }
}

