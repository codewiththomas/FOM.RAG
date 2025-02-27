using FOM.RAG.Demonstrator.Models;

namespace FOM.RAG.Demonstrator.Services;

public class InMemoryVectorStore
{
    private readonly List<DocumentEmbedding> _embeddings = new();


    public void AddEmbedding(DocumentEmbedding embedding)
    {
        _embeddings.Add(embedding);
    }


    public List<(DocumentEmbedding Embedding, float Score)> FindSimilar(ReadOnlyMemory<float> queryVector, int limit = 5)
    {
        return _embeddings
            .Select(embedding => (
                Embedding: embedding,
                Score: CosineSimilarity(queryVector, embedding.Vector)
            ))
            .OrderByDescending(x => x.Score)
            .Take(limit)
            .ToList();
    }


    private float CosineSimilarity(ReadOnlyMemory<float> a, ReadOnlyMemory<float> b)
    {
        var aSpan = a.Span;
        var bSpan = b.Span;

        float dotProduct = 0;
        float aMagnitude = 0;
        float bMagnitude = 0;

        for (int i = 0; i < aSpan.Length; i++)
        {
            dotProduct += aSpan[i] * bSpan[i];
            aMagnitude += aSpan[i] * aSpan[i];
            bMagnitude += bSpan[i] * bSpan[i];
        }

        return dotProduct / (float)(Math.Sqrt(aMagnitude) * Math.Sqrt(bMagnitude));
    }

}
