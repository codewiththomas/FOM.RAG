using System.Text.Json.Serialization;

namespace Zendesk.ApiClient.Endpoints.HelpCenter.Categories.GetList;

public class CategoryAsListItemDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("html_url")]
    public string? HtmlUrl { get; set; }


    [JsonPropertyName("position")]
    public int Position { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("locale")]
    public string? Locale { get; set; }


    [JsonPropertyName("source_locale")]
    public string? SourceLocale { get; set; }

    [JsonPropertyName("outdated")]
    public bool Outdated { get; set; }
}
