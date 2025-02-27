using System.Text.Json.Serialization;
using Zendesk.ApiClient.Pagination;

namespace Zendesk.ApiClient.Endpoints.HelpCenter.Categories.GetList;

public sealed class GetCategoryListResponse : PaginatedResponse
{
    [JsonPropertyName("categories")]
    public ICollection<CategoryAsListItemDto>? Categories { get; set; }
}
