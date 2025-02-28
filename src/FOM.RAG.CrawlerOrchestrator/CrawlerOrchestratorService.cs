using FOM.RAG.CrawlerOrchestrator.Abstractions.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Zendesk.ApiCrawler;

namespace FOM.RAG.CrawlerOrchestrator;

public class CrawlerOrchestratorService(
    [FromKeyedServices($"CRAWLER:{nameof(ZendeskCrawler)}")] ICrawler zendeskCrawler)
{
    public async Task<IEnumerable<IDocument>> CrawlAllAsync()
    {
        var documents = new List<IDocument>();
        documents.AddRange(await zendeskCrawler.CrawlAsync());
        return documents;
    }
}
