using Zendesk.ApiClient.Endpoints.HelpCenter;

namespace Zendesk.ApiClient.Abstractions;

public interface IZendeskClient
{
    public HelpCenterHandler HelpCenter { get; init; }
}
