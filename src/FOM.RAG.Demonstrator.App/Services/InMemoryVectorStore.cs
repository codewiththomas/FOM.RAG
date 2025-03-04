using FOM.RAG.Demonstrator.App.Models;
using System.Diagnostics;
using System.Numerics;

namespace FOM.RAG.Demonstrator.App.Services;

public class InMemoryVectorStore(ILogger<InMemoryVectorStore> logger)
{
    private readonly List<DocumentEmbedding> _embeddings = new();


    public void AddEmbedding(DocumentEmbedding embedding)
    {
        logger.LogInformation("Embedding added to store");
        _embeddings.Add(embedding);
    }


    public List<(DocumentEmbedding Embedding, float Score)> FindSimilar(ReadOnlyMemory<float> queryVector, int limit = 5)
    {
        logger.LogInformation("Vector Store: Try to find similar...");

        //Stopwatch
        var sw = new Stopwatch();
        sw.Start();
        var embeddings = _embeddings
            .AsParallel()
            .Select(embedding => (
                Embedding: embedding,
                Score: CosineSimilarity_v3(queryVector, embedding.Vector)
            ))
            .OrderByDescending(x => x.Score)
            .Take(limit)
            .ToList();
        sw.Stop();
        logger.LogInformation("Vector Store: Found similar embeddings in {time} ms", sw.ElapsedMilliseconds);

        return embeddings;
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


    private float CosineSimilarity_v2(ReadOnlyMemory<float> a, ReadOnlyMemory<float> b)
    {
        var aSpan = a.Span;
        var bSpan = b.Span;

        float dotProduct = 0;
        float aMagnitude = 0; 
        float bMagnitude = 0;
        int length = aSpan.Length;

        for (int i = 0; i < length; i++)
        {
            float aVal = aSpan[i];
            float bVal = bSpan[i];

            dotProduct += aVal * bVal;
            aMagnitude += aVal * aVal;
            bMagnitude += bVal * bVal;
        }

        float denominator = (float)(Math.Sqrt(aMagnitude) * Math.Sqrt(bMagnitude));
        return denominator == 0 ? 0 : dotProduct / denominator;
    }


    private float CosineSimilarity_v3(ReadOnlyMemory<float> a, ReadOnlyMemory<float> b)
    {
        var aSpan = a.Span;
        var bSpan = b.Span;

        int length = aSpan.Length;
        int simdLength = Vector<float>.Count;

        float dotProduct = 0, aMagnitude = 0, bMagnitude = 0;

        int i = 0;
        for (; i <= length - simdLength; i += simdLength)
        {
            var va = new Vector<float>(aSpan.Slice(i, simdLength));
            var vb = new Vector<float>(bSpan.Slice(i, simdLength));

            dotProduct += Vector.Dot(va, vb);
            aMagnitude += Vector.Dot(va, va);
            bMagnitude += Vector.Dot(vb, vb);
        }

        // Process remaining elements
        for (; i < length; i++)
        {
            float aVal = aSpan[i];
            float bVal = bSpan[i];
            dotProduct += aVal * bVal;
            aMagnitude += aVal * aVal;
            bMagnitude += bVal * bVal;
        }

        float denominator = (float)(Math.Sqrt(aMagnitude) * Math.Sqrt(bMagnitude));
        return denominator == 0 ? 0 : dotProduct / denominator;
    }

}
