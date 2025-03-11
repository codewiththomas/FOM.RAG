namespace FOM.RAG.Demonstrator.Models;

public class SectionChunk
{
    public string DocumentTitle { get; set; } = string.Empty;

    public string HierarchyNumber { get; set; } = string.Empty;

    public string[] ParentSectionTitles { get; set; } = [];

    public string SectionTitle { get; set; } = string.Empty;

    public string PlainTextContent { get; set; } = string.Empty;
}
