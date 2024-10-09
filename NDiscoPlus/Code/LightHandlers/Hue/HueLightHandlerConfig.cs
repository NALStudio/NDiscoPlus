using NDiscoPlus.Components.Elements.LightHandlerConfigEditor.HueLightHandlerConfigEditor;
using NDiscoPlus.PhilipsHue.Authentication.Models;

namespace NDiscoPlus.Code.LightHandlers.Hue;

public class HueLightHandlerConfig : LightHandlerConfig
{
    public string? BridgeIP { get; set; } = null;
    public HueCredentials? BridgeCredentials { get; set; } = null;
    public Guid? EntertainmentConfiguration { get; set; } = null;

    public override LightHandler CreateLightHandler()
        => new HueLightHandler(this);

    public override Type GetEditorType()
        => typeof(HueLightHandlerConfigEditor);
}