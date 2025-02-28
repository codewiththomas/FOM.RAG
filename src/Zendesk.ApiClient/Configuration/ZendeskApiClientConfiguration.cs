namespace Zendesk.ApiClient.Configuration;

public class ZendeskApiClientConfiguration
{
    /// <summary>
    /// Will be used to build the base URL for the API. Will resolve to https://{subdomain}.zendesk.com/api/v2/{endpoint}
    /// </summary>
    public required string Tenant { get; set; }

    /// <summary>
    /// For sake of simplicity, we will use a token for authentication. Use postman to generate a token.
    /// </summary>
    public required string Token { get; set; }
}
