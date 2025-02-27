using FOM.RAG.CrawlerOrchestrator.Abstractions.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zendesk.ApiClient;

namespace Zendesk.ApiCrawler;

public static class ServiceRegistration
{
    public static IServiceCollection AddZendeskCrawler(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddZendeskApiClient(configuration);
        services.AddKeyedScoped<ICrawler, ZendeskCrawler>($"CRAWLER:{nameof(ZendeskCrawler)}");
        return services;
    }
}
