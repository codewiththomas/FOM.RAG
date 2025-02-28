using System.Text.Json.Serialization;

namespace Zendesk.ApiClient.Endpoints.HelpCenter.Sections.GetList;

public sealed class SectionAsListItemDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("html_url")]
    public string? HtmlUrl { get; set; }

    [JsonPropertyName("category_id")]
    public long CategoryId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("parent_section_id")]
    public long? ParentSectionId { get; set; }

    [JsonPropertyName("theme_template")]
    public string? ThemeTemplate { get; set; }
}

