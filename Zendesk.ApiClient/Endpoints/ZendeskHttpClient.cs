namespace Zendesk.ApiClient.Endpoints;

public class ZendeskHttpClient
{
    private readonly HttpClient _client;

    public ZendeskHttpClient(HttpClient client)
    {
        _client = client;
    }

    public Task<HttpResponseMessage> GetAsync(string url, CancellationToken cancellationToken = default!)
    {
        string absoluteUrl = string.Empty;
        if (url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            absoluteUrl = url;
        }
        else
        {
            url = url.StartsWith("/") ? url.Substring(1) : url;
            absoluteUrl = _client.BaseAddress + url;
        }
        Console.WriteLine($"Requesting {absoluteUrl}");
        return _client.GetAsync(absoluteUrl, cancellationToken);
    }

}
