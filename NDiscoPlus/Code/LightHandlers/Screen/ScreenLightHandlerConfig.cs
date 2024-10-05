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

    public int AspectRatioHorizontal { get; set; } = 16;
    public int AspectRatioVertical { get; set; } = 9;

    [JsonIgnore]
    public double AspectRatio => AspectRatioHorizontal / (double)AspectRatioVertical;
    [JsonIgnore]
    public double InverseAspectRatio => AspectRatioVertical / (double)AspectRatioHorizontal;

    public override LightHandler CreateLightHandler()
        => new ScreenLightHandler(this);
    public override Type GetEditorType() => typeof(ScreenLightHandlerConfigEditor);
}