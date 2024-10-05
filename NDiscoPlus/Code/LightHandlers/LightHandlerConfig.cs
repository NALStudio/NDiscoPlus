using Microsoft.AspNetCore.Components;
using NDiscoPlus.Code.LightHandlers.Hue;
using NDiscoPlus.Code.LightHandlers.Screen;
using System.Text.Json.Serialization;

namespace NDiscoPlus.Code.LightHandlers;

[JsonDerivedType(typeof(ScreenLightHandlerConfig), typeDiscriminator: "screen")]
[JsonDerivedType(typeof(HueLightHandlerConfig), typeDiscriminator: "hue")]
public abstract class LightHandlerConfig
{
    /// <summary>
    /// This method is called to create a light handler from the given configuration.
    /// </summary>
    public abstract LightHandler CreateLightHandler();
    public abstract Type GetEditorType();
}