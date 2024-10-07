using NDiscoPlus.PhilipsHue.Api.Models.Responses;
using NDiscoPlus.PhilipsHue.Api.Models.Responses.Entertainment;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Api.HueApi;
public partial class LocalHueApi
{
    public async Task<IList<EntertainmentConfigurationGet>> GetEntertainmentConfigurations() => await GetAsync<ImmutableArray<EntertainmentConfigurationGet>>(Endpoints.EntertainmentConfigurations);
    public async Task<EntertainmentConfigurationGet?> GetEntertainmentConfiguration(Guid id) => await GetSingleOrDefaultAsync<EntertainmentConfigurationGet>(Endpoints.EntertainmentConfiguration(id));
}
