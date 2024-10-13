using NDiscoPlus.Code.JavaScriptInterop;
using NDiscoPlus.Shared.Models;
using NDiscoPlus.Shared.Models.Color;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.Code.LightHandlers.Screen;
internal abstract class BaseScreenLightHandler : LightHandler
{
    protected record struct RenderMeta(Type ComponentType, bool IncludeWidthHeightDataInArgs = true);
    public record struct RenderData(Type ComponentType, ReadOnlyDictionary<string, object> ComponentArgs);

    protected BaseScreenLightHandler(BaseScreenLightHandlerConfig? config) : base(config)
    {
        componentArgs = new();
    }

    private readonly Dictionary<string, object> componentArgs;
    private RenderMeta? render;

    public RenderData? Render(int width, int height)
    {
        if (render is not RenderMeta r)
            return null;

        if (r.IncludeWidthHeightDataInArgs)
        {
            componentArgs["Width"] = width;
            componentArgs["Height"] = height;
        }

        return new RenderData(r.ComponentType, componentArgs.AsReadOnly());
    }

    public sealed override ValueTask Update(LightColorCollection lights)
    {
        // tmp variable for ref
        Dictionary<string, object> args = componentArgs;

        render = RenderUpdate(lights, ref args);
        return ValueTask.CompletedTask;
    }

    protected abstract RenderMeta? RenderUpdate(LightColorCollection lights, ref readonly Dictionary<string, object> componentArgs);
}
