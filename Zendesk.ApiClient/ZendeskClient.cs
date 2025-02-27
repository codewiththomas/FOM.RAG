using Zendesk.ApiClient.Abstractions;
using Zendesk.ApiClient.Endpoints.HelpCenter;

namespace Zendesk.ApiClient;


public class ZendeskClient(HelpCenterHandler helpCenterHandler) : IZendeskClient
{
    public HelpCenterHandler HelpCenter { get; init; } = helpCenterHandler;
}
