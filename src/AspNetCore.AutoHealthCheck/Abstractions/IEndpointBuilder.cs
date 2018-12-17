using Microsoft.AspNetCore.Http;

namespace AspNetCore.AutoHealthCheck
{
    internal interface IEndpointBuilder
    {
        Endpoint CreateFromRoute(IRouteInformation routeInformation);
    }
}