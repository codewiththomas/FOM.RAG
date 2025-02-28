using System.Text.Json.Serialization;

namespace Zendesk.ApiClient.Endpoints.HelpCenter.Articles.GetList;

public class ArcticleAsListItemDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("html_url")]
    public string? HtmlUrl { get; set; }


    [JsonPropertyName("author_id")]
    public long AuthorId { get; set; }

    [JsonPropertyName("commend_disabled")]
    public bool CommentDisabled { get; set; }

    [JsonPropertyName("draft")]
    public bool Draft { get; set; }

    [JsonPropertyName("promoted")]
    public bool Promoted { get; set; }

    [JsonPropertyName("position")]
    public int Position { get; set; }

    [JsonPropertyName("vote_sum")]
    public int VoteSum { get; set; }

    [JsonPropertyName("vote_count")]
    public int VoteCount { get; set; }

    [JsonPropertyName("section_id")]
    public long SectionId { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("source_locale")]
    public string? SourceLocale { get; set; }

    [JsonPropertyName("locale")]
    public string? Locale { get; set; }

    [JsonPropertyName("outdated")]
    public bool Outdated { get; set; }

    //outdated_locales

    [JsonPropertyName("edited_at")]
    public DateTime EditedAt { get; set; }

    [JsonPropertyName("permission_group_id")]
    public long? PermissionGroupId { get; set; }

    //label_names

    [JsonPropertyName("body")]
    public string? Body { get; set; }

}
