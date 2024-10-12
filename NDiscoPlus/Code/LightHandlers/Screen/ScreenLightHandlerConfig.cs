using NDiscoPlus.Components.Elements.LightHandlerConfigEditor;
using System.Text.Json.Serialization;

namespace NDiscoPlus.Code.LightHandlers.Screen;

public enum ScreenLightCount
{
    Four = 4,
    Six = 6
}

public class ScreenLightHandlerConfig : LightHandlerConfig
{
    public ScreenLightCount LightCount { get; set; } = ScreenLightCount.Six;
    public bool UseHDR { get; set; } = false;

    public ScreenLightMetrics LightMetrics { get; init; } = new();

    public override LightHandler CreateLightHandler()
        => new ScreenLightHandler(this);
    public override Type GetEditorType() => typeof(ScreenLightHandlerConfigEditor);
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