using FOM.RAG.CrawlerOrchestrator.Abstractions.Contracts;

namespace FOM.RAG.CrawlerOrchestrator.Abstractions.Dtos;

public class Document : IDocument
{
    public Guid DocumentId { get; set; }
    public string Origin { get; set; } = string.Empty;
    public string IdInOrigin { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string Title { get; set; } = string.Empty;
    public string[] Categories { get; set; } = Array.Empty<string>();
    public string HtmlBody { get; set; } = string.Empty;
}
