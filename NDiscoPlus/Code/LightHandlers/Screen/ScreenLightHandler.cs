﻿using MudBlazor;
using NDiscoPlus.Components.Elements;
using NDiscoPlus.Shared.Models;
using NDiscoPlus.Shared.Models.Color;
using System.Diagnostics;
using System.Runtime.Versioning;

namespace NDiscoPlus.Code.LightHandlers.Screen;

internal readonly record struct ScreenLight(NDPLight Light, NDPColor? Color)
{
    public ScreenLight ReplaceColor(NDPColor color)
        => new(Light, color);
}

internal class ScreenLightHandler : LightHandler<ScreenLightHandlerConfig>
{
    private record class LightsContainer(NDPLight[] Lights)
    {
        public NDPColor[]? Colors { get; set; } = null;
    }

    private LightsContainer? lights;
    public class RenderData
    {
        private readonly ScreenLightHandler parent;
        public RenderData(ScreenLightHandler parent)
        {
            this.parent = parent;
        }

        public IReadOnlyList<NDPColor>? Colors => parent.lights?.Colors;
        public bool HDR => parent.Config.UseHDR;
    }


    // Publicly exposed parameters for screen light handling
    public RenderData Render { get; }
    public ScreenLightSignaler? Signaler { get; set; }

    public ScreenLightHandler(ScreenLightHandlerConfig? config) : base(config)
    {
        Render = new RenderData(this);
    }

    protected override ScreenLightHandlerConfig CreateConfig()
        => new();

    private NDPLight[] GetLights4()
    {
        return new NDPLight[]
        {
            CreateLight(new ScreenLightId(4, 0), new LightPosition(-0.5d, 0.5d, 0.5d)),
            CreateLight(new ScreenLightId(4, 1), new LightPosition(0.5d, 0.5d, 0.5d)),
            CreateLight(new ScreenLightId(4, 2), new LightPosition(-0.5d, -0.5d, -0.5d)),
            CreateLight(new ScreenLightId(4, 3), new LightPosition(0.5d, -0.5d, -0.5d))
        };
    }

    private NDPLight[] GetLights6()
    {
        return new NDPLight[]
        {
            CreateLight(new ScreenLightId(6, 0), new LightPosition(-0.5d, 0.5d, 0.5d)),
            CreateLight(new ScreenLightId(6, 1), new LightPosition(0, 0.5d, 0.5d)),
            CreateLight(new ScreenLightId(6, 2), new LightPosition(0.5d, 0.5d, 0.5d)),
            CreateLight(new ScreenLightId(6, 3), new LightPosition(-0.5d, -0.5d, -0.5d)),
            CreateLight(new ScreenLightId(6, 4), new LightPosition(0, -0.5d, -0.5d)),
            CreateLight(new ScreenLightId(6, 5), new LightPosition(0.5d, -0.5d, -0.5d))
        };
    }

    private NDPLight CreateLight(ScreenLightId id, LightPosition pos)
    {
        ColorGamut colorGamut = Config.UseHDR ? ColorGamut.DisplayP3 : ColorGamut.sRGB;

        string leftRight = pos.X switch
        {
            < 0 => "Left",
            > 0 => "Right",
            _ => "Mid"
        };
        string topBottom = pos.Y switch
        {
            < 0 => "Bottom",
            > 0 => "Top",
            _ => "Mid"
        };

        return new()
        {
            Id = id,
            DisplayName = $"{topBottom}-{leftRight}",
            Position = pos,
            ColorGamut = colorGamut,
            ExpectedLatency = TimeSpan.Zero
        };
    }

    private NDPLight[] GetLightsInternal()
    {
        if (Config.LightCount == ScreenLightCount.Four)
            return GetLights4();
        else if (Config.LightCount == ScreenLightCount.Six)
            return GetLights6();
        else
            throw new InvalidLightHandlerConfigException("Invalid light count.");
    }

    public override async IAsyncEnumerable<NDPLight> GetLights()
    {
        foreach (NDPLight l in GetLightsInternal())
        {
            await Task.Delay(50);
            yield return l;
        }
    }

    public override ValueTask<bool> ValidateConfig(ErrorMessageCollector? errors)
    {
        bool valid = true;

        if (!Enum.GetValues<ScreenLightCount>().Contains(Config.LightCount))
        {
            errors?.Add($"Invalid Screen Light Count: {Config.LightCount}");
            valid = false;
        }

        return new ValueTask<bool>(valid);
    }

    public override ValueTask<NDPLight[]?> Start(ErrorMessageCollector? errors)
    {
        if (lights is not null)
        {
            errors?.Add("Handler already running.");
            return new((NDPLight[]?)null);
        }

        NDPLight[] l = GetLightsInternal();
        lights = new LightsContainer(l);

        return new(l);
    }

    public override ValueTask Update(LightColorCollection lightColors)
    {
        if (lights is null)
            throw new InvalidOperationException("Screen Light Handler not started.");
        lights.Colors ??= new NDPColor[lights.Lights.Length];

        foreach ((ScreenLightId light, NDPColor color) in lightColors.OfType<ScreenLightId>()) // get lights of correct type
            lights.Colors[light.Index] = color;

        return new();
    }

    public override ValueTask Stop()
    {
        lights = null;
        return new();
    }

    public override ValueTask<TimeSpan?> Signal(LightId lightId, NDPColor color)
    {
        ScreenLightId light = (ScreenLightId)lightId;

        if (Signaler is null)
            return new((TimeSpan?)null);

        int lightIndex = light.Index;
        int lightCount = light.TotalLightCount;

        const int rowCount = 2;
        int lightsPerRow = lightCount / rowCount;
        Debug.Assert((lightsPerRow * rowCount) == lightCount);

        (int yIndex, int xIndex) = Math.DivRem(lightIndex, lightsPerRow);

        double x = xIndex / (double)(lightsPerRow - 1);
        double y = yIndex / (double)(rowCount - 1);
        Signaler.Signal(x, y, color);

        return new(ScreenLightSignaler.AnimationDuration);
    }
}
