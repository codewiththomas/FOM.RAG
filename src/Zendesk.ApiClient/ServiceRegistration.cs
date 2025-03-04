using Zendesk.ApiClient.Configuration;
using Zendesk.ApiClient.Endpoints.HelpCenter.Articles;
using Zendesk.ApiClient.Endpoints.HelpCenter.Categories;
using Zendesk.ApiClient.Endpoints.HelpCenter.Sections;
using Zendesk.ApiClient.Endpoints.HelpCenter;
using Zendesk.ApiClient.Endpoints;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using Zendesk.ApiClient.Abstractions;

namespace Zendesk.ApiClient;

public static class ServiceRegistration
{
    public static IServiceCollection AddZendeskApiClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ZendeskApiClientConfiguration>(
            configuration.GetSection("ZendeskApiClientConfiguration"));

        services.AddHttpClient<ZendeskHttpClient>((provider, client) =>
        {
            var config = provider.GetRequiredService<IOptions<ZendeskApiClientConfiguration>>().Value;
            var tenant = config.Tenant.Trim();
            var url = $"https://{tenant}.zendesk.com/api/v2/";
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", config.Token);
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        });

        services.AddScoped<ArticleEndpointsHandler>();
        services.AddScoped<CategoryEndpointsHandler>();
        services.AddScoped<SectionEndpointsHandler>();

        services.AddScoped<HelpCenterHandler>();

        services.AddScoped<IZendeskClient, ZendeskClient>();

        return services;
    }
}
