namespace FOM.RAG.CrawlerOrchestrator.Abstractions.Contracts;

public interface ICrawler
{
    public IReadOnlyList<string> IncludeArticleIds { get; }
    public IReadOnlyList<string> ExcludeArticleIds { get; }


    /// <summary>
    /// Starts the crawling process.
    /// </summary>
    /// <returns></returns>
    public Task<IEnumerable<IDocument>> CrawlAsync();


    /// <summary>
    /// If set, only articles with these ids will be included in the crawl.
    /// Can not be used in combination with ExcludeArticleId.
    /// </summary>
    /// <param name="articleId"></param>
    public void IncludeArticleId(string articleId);


    /// <summary>
    /// If set, only articles with these ids will be excluded from the crawl.
    /// Can not be used in combination with IncludeArticleId.
    /// </summary>
    /// <param name="articleId"></param>
    public void ExcludeArticleId(string articleId);
}
