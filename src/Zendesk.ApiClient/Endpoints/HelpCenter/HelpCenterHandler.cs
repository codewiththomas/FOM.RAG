using Zendesk.ApiClient.Endpoints.HelpCenter.Articles;
using Zendesk.ApiClient.Endpoints.HelpCenter.Categories;
using Zendesk.ApiClient.Endpoints.HelpCenter.Sections;

namespace Zendesk.ApiClient.Endpoints.HelpCenter;

public class HelpCenterHandler(
    ArticleEndpointsHandler articleEndpointsHandler,
    CategoryEndpointsHandler categoryEndpointsHandler,
    SectionEndpointsHandler sectionEndpointsHandler)
{
    public ArticleEndpointsHandler Articles { get; } = articleEndpointsHandler;

    public CategoryEndpointsHandler Categories { get; } = categoryEndpointsHandler;

    public SectionEndpointsHandler Sections { get; } = sectionEndpointsHandler;
}
