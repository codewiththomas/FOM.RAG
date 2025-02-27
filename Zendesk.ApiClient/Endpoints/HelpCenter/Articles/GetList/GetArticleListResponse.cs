using System.Text.Json.Serialization;
using Zendesk.ApiClient.Pagination;

namespace Zendesk.ApiClient.Endpoints.HelpCenter.Articles.GetList;

public sealed class GetArticleListResponse : PaginatedResponse
{
    [JsonPropertyName("articles")]
    public ICollection<ArcticleAsListItemDto>? Articles { get; set; }
}
