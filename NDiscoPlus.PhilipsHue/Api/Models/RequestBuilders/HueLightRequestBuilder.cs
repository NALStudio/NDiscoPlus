using NDiscoPlus.PhilipsHue.Api.Models.RequestBuilders.Base;
using NDiscoPlus.PhilipsHue.Api.Models.Responses.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Api.Models.RequestBuilders;
public class HueLightRequestBuilder : HueRequestBuilder
{
    // TODO: Implement archetype, function
    public HueLightRequestBuilder WithMetadata(string? name = null)
    {
        using PropertyBuilder pb = BuildProperty("metadata");
        pb.AddPropertyIfNotNull("name", name);

        return this;
    }

    public HueLightRequestBuilder WithOn(bool on)
    {
        AddProperty("on", "on", on);
        return this;
    }

    public HueLightRequestBuilder WithDimming(double brightness)
    {
        AddProperty("dimming", "brightness", brightness);
        return this;
    }

    public HueLightRequestBuilder WithColorTemperature(int mirek)
    {
        AddProperty("color_temperature", "mirek", mirek);
        return this;
    }

    public HueLightRequestBuilder WithColor(HueXY color)
    {
        AddProperty("color", color.ToObjectDictionary());
        return this;
    }

    public HueLightRequestBuilder WithDynamics(int? duration = null, double? speed = null)
    {
        using PropertyBuilder pb = BuildProperty("dynamics");
        pb.AddPropertyIfNotNull("duration", duration);
        pb.AddPropertyIfNotNull("speed", speed);

        return this;
    }

    /// <summary>
    /// Stop the currently active signal.
    /// </summary>
    public HueLightRequestBuilder WithSignaling(bool stop)
    {
        if (stop)
            _ = InternalWithSignaling("no_signal", 0);
        return this;
    }

    /// <summary>
    /// <para>Start an on-off signal.</para>
    /// <para>Toggles between max brightness and Off in fixed color.</para>
    /// </summary>
    public HueLightRequestBuilder WithSignaling(int durationMs) => InternalWithSignaling("on_off", durationMs);
    public HueLightRequestBuilder WithSignaling(TimeSpan duration) => WithSignaling((int)duration.TotalMilliseconds);

    /// <summary>
    /// <para>Start an on-off signal with color.</para>
    /// <para>Toggles between off and max brightness with color provided.</para>
    /// </summary>
    public HueLightRequestBuilder WithSignaling(int durationMs, HueXY color) => InternalWithSignaling("on_off_color", durationMs, color);
    public HueLightRequestBuilder WithSignaling(TimeSpan duration, HueXY color) => WithSignaling((int)duration.TotalMilliseconds, color);

    /// <summary>
    /// <para>Start an alternating signal.</para>
    /// <para>Alternates between 2 provided colors.</para>
    /// </summary>
    public HueLightRequestBuilder WithSignaling(int durationMs, HueXY color1, HueXY color2) => InternalWithSignaling("alternating", durationMs, color1, color2);
    public HueLightRequestBuilder WithSignaling(TimeSpan duration, HueXY color1, HueXY color2) => WithSignaling((int)duration.TotalMilliseconds, color1, color2);

    private HueLightRequestBuilder InternalWithSignaling(string signal, int durationMs, params HueXY[] colors)
    {
        using PropertyBuilder pb = BuildProperty("signaling");
        pb.AddProperty("signal", signal);
        pb.AddProperty("duration", durationMs);

        if (colors.Length > 0)
            pb.AddProperty("colors", colors);

        return this;
    }

    // TODO: Implement rest of the functionality
}
