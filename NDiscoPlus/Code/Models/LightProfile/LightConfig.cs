using NDiscoPlus.Shared.Effects.API.Channels.Effects.Intrinsics;
using NDiscoPlus.Shared.Models;

namespace NDiscoPlus.Code.Models.LightProfile;
public class LightConfig
{
    public Channel Channel { get; set; } = LightRecord.Default.Channel;
    public double Brightness { get; set; } = LightRecord.Default.Brightness;

    public LightRecord CreateRecord(NDPLight light)
    {
        return new(light)
        {
            Channel = Channel,
            Brightness = Brightness,
        };
    }
}