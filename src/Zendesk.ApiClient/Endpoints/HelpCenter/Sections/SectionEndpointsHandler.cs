using System.Net.Http.Json;
using Zendesk.ApiClient.Endpoints.HelpCenter.Sections.GetList;
using Zendesk.ApiClient.Pagination;

namespace Zendesk.ApiClient.Endpoints.HelpCenter.Sections;

public class SectionEndpointsHandler(ZendeskHttpClient client)
{

    const string ENDPOINT = "/help_center/sections";

    public async Task<ICollection<SectionAsListItemDto>> GetListAsync(int? maxPages = null)
    {
        string? nextUrl = ENDPOINT + "?per_page=100";
        var allItems = new List<SectionAsListItemDto>();

        int counter = 0;

        do
        {
            counter++;

            var pagedResult = await GetListPageAsync(nextUrl);

            if (pagedResult is not PaginatedResponse)
            {
                break;
            }

            if (pagedResult.Sections is ICollection<SectionAsListItemDto> pagedItems)
            {
                allItems.AddRange(pagedItems);
            }

            if (string.IsNullOrWhiteSpace(pagedResult.NextPage))
            {
                break;
            }

            nextUrl = pagedResult.NextPage;

        } while (true && (maxPages == null || maxPages.HasValue && counter < maxPages.Value));

        return allItems;
    }


    private async Task<GetSectionListResponse?> GetListPageAsync(string? url)
    {
        HttpResponseMessage response = await client.GetAsync(url ?? ENDPOINT);

        if (response.IsSuccessStatusCode)
        {
            var pageResult = await response.Content.ReadFromJsonAsync<GetSectionListResponse>();
            return pageResult;
        }

        Console.WriteLine("Request failed: " + response.StatusCode);
        return null;
    }
}

