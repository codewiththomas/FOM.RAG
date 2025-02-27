using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zendesk.ApiCrawler;

namespace FOM.RAG.CrawlerOrchestrator;

public static class ServiceRegistration
{
    public static IServiceCollection AddCrawlers(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddZendeskCrawler(configuration);
        services.AddScoped<CrawlerOrchestratorService>();
        return services;
    }
}
