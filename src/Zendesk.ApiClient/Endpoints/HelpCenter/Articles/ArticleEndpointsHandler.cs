using System.Net.Http.Json;
using Zendesk.ApiClient.Endpoints.HelpCenter.Articles.GetList;
using Zendesk.ApiClient.Pagination;

namespace Zendesk.ApiClient.Endpoints.HelpCenter.Articles;

public class ArticleEndpointsHandler(ZendeskHttpClient client)
{
    const string ENDPOINT = "/help_center/articles";

    public async Task<bool> ExistsAsync(long articleId)
    {
        var relativeUrl = $"{ENDPOINT}/{articleId}";
        HttpResponseMessage response = await client.GetAsync(relativeUrl);

        if (response.IsSuccessStatusCode)
        {
            return true;
        }

        return false;
    }


    public async Task<ICollection<ArcticleAsListItemDto>> GetListAsync(int? maxPages = null)
    {
        string? nextUrl = ENDPOINT + "?per_page=100";
        var allArticles = new List<ArcticleAsListItemDto>();

        int counter = 0;

        do
        {
            counter++;

            var pagedResult = await GetListPageAsync(nextUrl);

            if (pagedResult is not PaginatedResponse)
            {
                break;
            }

            if (pagedResult.Articles is ICollection<ArcticleAsListItemDto> articles)
            {
                allArticles.AddRange(articles);
            }

            if (string.IsNullOrWhiteSpace(pagedResult.NextPage))
            {
                break;
            }

            nextUrl = pagedResult.NextPage;

        } while (true && (maxPages == null || maxPages.HasValue && counter < maxPages.Value));

        return allArticles;
    }


    private async Task<GetArticleListResponse?> GetListPageAsync(string? url)
    {
        HttpResponseMessage response = await client.GetAsync(url ?? ENDPOINT);

        if (response.IsSuccessStatusCode)
        {
            var pageResult = await response.Content.ReadFromJsonAsync<GetArticleListResponse>();
            return pageResult;
        }

        Console.WriteLine("Request failed: " + response.StatusCode);
        return null;
    }
}

