using NDiscoPlus.PhilipsHue.Api.Models.RequestBuilders.Base;
using NDiscoPlus.PhilipsHue.Api.Models.Responses.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Api.Models.RequestBuilders;
public class HueEntertainmentConfigurationRequestBuilder : HueRequestBuilder
{
    public HueEntertainmentConfigurationRequestBuilder AddLocation(HueResourceIdentifier service, HuePosition position)
    {
        object location = CreateLocation(service, position);

        object OnAdd() => new List<object> { location };
        object OnUpdate(object values)
        {
            // TODO: Verify no duplicate values
            ((List<object>)values).Add(location);
            return values;
        }

        AddOrUpdateProperty(
            "locations",
            "service_locations",
            onAdd: OnAdd,
            onUpdate: OnUpdate
        );

        return this;
    }

    private static Dictionary<string, object> CreateLocation(HueResourceIdentifier service, HuePosition position)
    {
        return new Dictionary<string, object>()
        {
            { "service", service },
            { "positions", new HuePosition[] { position } },
        };
    }
}
