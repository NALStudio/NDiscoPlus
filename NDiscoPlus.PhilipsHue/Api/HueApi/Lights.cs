using NDiscoPlus.PhilipsHue.Api.Models.RequestBuilders;
using NDiscoPlus.PhilipsHue.Api.Models.RequestBuilders.Base;
using NDiscoPlus.PhilipsHue.Api.Models.Responses.Lights;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Api.HueApi;
public partial class LocalHueApi
{
    public async Task<IList<HueLight>> GetLightsAsync() => await GetAsync<ImmutableArray<HueLight>>(Endpoints.Lights);
    public async Task<HueLight?> GetLightAsync(Guid id) => await GetSingleOrDefaultAsync<HueLight>(Endpoints.Light(id));
    public async Task UpdateLightAsync(Guid id, HueLightRequestBuilder request) => await PutAsync(Endpoints.Light(id), request);
}