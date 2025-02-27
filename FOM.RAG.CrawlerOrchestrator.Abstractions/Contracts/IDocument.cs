namespace FOM.RAG.CrawlerOrchestrator.Abstractions.Contracts;

public interface IDocument
{
    public Guid DocumentId { get; set; }
    public string Origin { get; set; }
    public string IdInOrigin { get; set; }
    public string? Location { get; set; }
    public string Title { get; set; }
    public string[] Categories { get; set; }
    public string HtmlBody { get; set; }
}
