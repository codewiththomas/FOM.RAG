using System.Net.Http.Json;
using Zendesk.ApiClient.Endpoints.HelpCenter.Categories.GetList;
using Zendesk.ApiClient.Pagination;

namespace Zendesk.ApiClient.Endpoints.HelpCenter.Categories;

public class CategoryEndpointsHandler(ZendeskHttpClient client)
{
    
    const string ENDPOINT = "/help_center/categories";

    public async Task<ICollection<CategoryAsListItemDto>> GetListAsync(int? maxPages = null)
    {
        string? nextUrl = ENDPOINT + "?per_page=100";
        var allItems = new List<CategoryAsListItemDto>();

        int counter = 0;

        do
        {
            counter++;

            var pagedResult = await GetListPageAsync(nextUrl);

            if (pagedResult is not PaginatedResponse)
            {
                break;
            }

            if (pagedResult.Categories is ICollection<CategoryAsListItemDto> pagedItems)
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


    private async Task<GetCategoryListResponse?> GetListPageAsync(string? url)
    {
        HttpResponseMessage response = await client.GetAsync(url ?? ENDPOINT);

        if (response.IsSuccessStatusCode)
        {
            var pageResult = await response.Content.ReadFromJsonAsync<GetCategoryListResponse>();
            return pageResult;
        }

        Console.WriteLine("Request failed: " + response.StatusCode);
        return null;
    }
}
