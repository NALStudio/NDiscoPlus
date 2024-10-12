using NDiscoPlus.PhilipsHue.Api.Constants;
using NDiscoPlus.PhilipsHue.Api.Models.RequestBuilders;
using NDiscoPlus.PhilipsHue.Api.Models.Responses;
using NDiscoPlus.PhilipsHue.Api.Models.Responses.Entertainment;
using NDiscoPlus.PhilipsHue.Api.Models.Responses.Generic;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Api.HueApi;
public partial class LocalHueApi
{
    public async Task<IList<HueEntertainmentConfiguration>> GetEntertainmentConfigurationsAsync() => await GetAsync<ImmutableArray<HueEntertainmentConfiguration>>(Endpoints.EntertainmentConfigurations);
    public async Task<HueEntertainmentConfiguration> GetEntertainmentConfigurationAsync(Guid id) => await GetSingleAsync<HueEntertainmentConfiguration>(Endpoints.EntertainmentConfiguration(id));
    public async Task<HueEntertainmentConfiguration?> TryGetEntertainmentConfigurationAsync(Guid id) => await TryGetSingleAsync<HueEntertainmentConfiguration>(Endpoints.EntertainmentConfiguration(id));
    public async Task UpdateEntertainmentConfigurationAsync(Guid id, HueEntertainmentConfigurationRequestBuilder request) => await PutAsync(Endpoints.EntertainmentConfiguration(id), request);

    public async Task<IList<HueEntertainmentService>> GetEntertainmentServicesAsync() => await GetAsync<ImmutableArray<HueEntertainmentService>>(Endpoints.EntertainmentServices);
    public async Task<HueEntertainmentService> GetEntertainmentService(Guid id) => await GetSingleAsync<HueEntertainmentService>(Endpoints.EntertainmentService(id));
    public async Task<HueEntertainmentService?> TryGetEntertainmentService(Guid id) => await TryGetSingleAsync<HueEntertainmentService>(Endpoints.EntertainmentService(id));

    // Internal, we don't want to expose the action parameter to users
    internal async Task UpdateEntertainmentConfigurationActionAsync(Guid id, string action)
    {
        ClipV2Endpoint endpoint = Endpoints.EntertainmentConfiguration(id);
        JsonContent content = JsonContent.Create(
            new Dictionary<string, string> { { "action", action } }
        );
        await PutAsync<ImmutableArray<HueResourceIdentifier>>(endpoint, content);
    }
}
