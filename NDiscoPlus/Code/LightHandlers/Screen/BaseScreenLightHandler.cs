using NDiscoPlus.Code.JavaScriptInterop;
using NDiscoPlus.Shared.Models;
using NDiscoPlus.Shared.Models.Color;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.Code.LightHandlers.Screen;
internal abstract class BaseScreenLightHandler : LightHandler
{
    protected record struct RenderMeta(Type ComponentType, object Colors);
    public record struct RenderData(Type ComponentType, ReadOnlyDictionary<string, object> ComponentArgs);

    protected BaseScreenLightHandler(BaseScreenLightHandlerConfig? config) : base(config)
    {
        componentArgsCachedMutable = new();
        componentArgsCachedReadonly = componentArgsCachedMutable.AsReadOnly();
    }

    private readonly object renderLock = new();
    private RenderMeta? render;

    private readonly Dictionary<string, object> componentArgsCachedMutable;
    private readonly ReadOnlyDictionary<string, object> componentArgsCachedReadonly;


    /// <summary>
    /// This method is safe to call from a different thread than <see cref="Update"/>,
    /// but all calls to this function must come from the same thread.
    /// </summary>
    public RenderData? Render(int width, int height)
    {
        RenderMeta? render;
        lock (renderLock)
        {
            render = this.render;
        }

        if (render is not RenderMeta meta)
            return null;

        Debug.Assert(meta.Colors.GetType().IsValueType, "Colors must be value types to be thread-safe.");

        componentArgsCachedMutable["Width"] = width;
        componentArgsCachedMutable["Height"] = height;
        componentArgsCachedMutable["Colors"] = meta.Colors;

        return new RenderData(
            meta.ComponentType,
            componentArgsCachedReadonly
        );
    }

    public sealed override void Update(LightColorCollection lights)
    {
        lock (renderLock)
        {
            render = RenderUpdate(lights);
        }
    }

    protected abstract RenderMeta? RenderUpdate(LightColorCollection lights);
}
