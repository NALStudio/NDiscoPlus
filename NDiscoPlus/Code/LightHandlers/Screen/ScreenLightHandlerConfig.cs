using NDiscoPlus.Components.Elements.LightHandlerConfigEditor;
using NDiscoPlus.Shared.Models;
using System.Text.Json.Serialization;

namespace NDiscoPlus.Code.LightHandlers.Screen;

public enum ScreenLightCount
{
    Four = 4,
    Six = 6
}


public enum ScreenHorizontalLightsVariant
{
    Continuous,
    FixedWidth
}

public abstract class BaseScreenLightHandlerConfig : LightHandlerConfig
{
    public bool UseHDR { get; set; } = false;

    [JsonIgnore]
    public ColorGamut ColorGamut => UseHDR ? ColorGamut.DisplayP3 : ColorGamut.sRGB;

    public ScreenLightMetrics LightMetrics { get; init; } = new();

    public sealed override Type GetEditorType() => typeof(ScreenLightHandlerConfigEditor);

    internal bool ValidateConfig(ErrorMessageCollector? errors)
    {
        bool valid = true;

        if (UseHDR)
        {
            valid = false;
            errors?.Add("HDR has not yet been implemented.");
        }

        if (LightMetrics.AspectRatioX < 1 || LightMetrics.AspectRatioY < 1)
        {
            valid = false;
            errors?.Add("Invalid aspect ratio.");
        }

        return valid;
    }
}

public class ScreenMimicLightHandlerConfig : BaseScreenLightHandlerConfig
{
    public ScreenHorizontalLightsVariant Variant { get; set; } = ScreenHorizontalLightsVariant.Continuous;
    public double BrightnessMultiplier { get; set; } = 0.25d;

    public override LightHandler CreateLightHandler()
        => new ScreenMimicLightHandler(this);
}

public class ScreenLightHandlerConfig : BaseScreenLightHandlerConfig
{
    public ScreenLightCount LightCount { get; set; } = ScreenLightCount.Six;

    public override LightHandler CreateLightHandler()
        => new ScreenLightHandler(this);
}

public class ScreenLightMetrics
{
    public int AspectRatioX { get; set; } = 16;
    public int AspectRatioY { get; set; } = 9;

    public double X { get; set; } = 0d;
    public double Y { get; set; } = 0.75d;
    public double Z { get; set; } = -0.1d;
    public double Height { get; set; } = 0.3d;

    [JsonIgnore]
    public double AspectRatio => AspectRatioX / (double)AspectRatioY;

    [JsonIgnore]
    public double Width => AspectRatio * Height;

    [JsonIgnore]
    public double Top => Z + (Height / 2d);

    [JsonIgnore]
    public double Bottom => Z - (Height / 2d);

    [JsonIgnore]
    public double Left => X - (Width / 2d);

    [JsonIgnore]
    public double Mid => X;

    [JsonIgnore]
    public double Right => X + (Width / 2d);
}