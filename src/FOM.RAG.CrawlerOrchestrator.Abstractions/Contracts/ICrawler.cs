namespace FOM.RAG.CrawlerOrchestrator.Abstractions.Contracts;

public interface ICrawler
{
    public Task<IEnumerable<IDocument>> CrawlAsync();
}
