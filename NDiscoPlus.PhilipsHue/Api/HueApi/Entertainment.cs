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
    public async Task<HueEntertainmentConfiguration?> GetEntertainmentConfigurationAsync(Guid id) => await GetSingleOrDefaultAsync<HueEntertainmentConfiguration>(Endpoints.EntertainmentConfiguration(id));

    // Internal, we don't want to expose the action parameter to users
    internal async Task UpdateEntertainmentConfigurationActionAsync(Guid id, string action)
    {
        string endpoint = Endpoints.EntertainmentConfiguration(id);
        JsonContent content = JsonContent.Create(
            new Dictionary<string, string> { { "action", action } }
        );
        await PutAsync<ImmutableArray<HueResourceIdentifier>>(endpoint, content);
    }
}
