using System.Text.Json.Serialization;
using Zendesk.ApiClient.Pagination;

namespace Zendesk.ApiClient.Endpoints.HelpCenter.Sections.GetList;

public class GetSectionListResponse : PaginatedResponse
{
    [JsonPropertyName("sections")]
    public ICollection<SectionAsListItemDto>? Sections { get; set; }
}
