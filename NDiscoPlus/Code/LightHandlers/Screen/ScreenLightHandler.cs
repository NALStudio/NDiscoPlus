using MudBlazor;
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

    private static NDPLight[] GetLights4(ColorGamut colorGamut)
    {
        return new NDPLight[]
        {
            new(new ScreenLightId(4, 0), "Top-Left", new LightPosition(-0.5d, 0.5d, 0.5d), colorGamut: colorGamut),
            new(new ScreenLightId(4, 1), "Top-Right", new LightPosition(0.5d, 0.5d, 0.5d), colorGamut: colorGamut),
            new(new ScreenLightId(4, 2), "Bottom-Left", new LightPosition(-0.5d, -0.5d, -0.5d), colorGamut: colorGamut),
            new(new ScreenLightId(4, 3), "Bottom-Right", new LightPosition(0.5d, -0.5d, -0.5d), colorGamut: colorGamut)
        };
    }

    private static NDPLight[] GetLights6(ColorGamut colorGamut)
    {
        return new NDPLight[]
        {
            new(new ScreenLightId(6, 0), "Top-Left", new LightPosition(-0.5d, 0.5d, 0.5d), colorGamut: colorGamut),
            new(new ScreenLightId(6, 1), "Top-Mid", new LightPosition(0, 0.5d, 0.5d), colorGamut: colorGamut),
            new(new ScreenLightId(6, 2), "Top-Right", new LightPosition(0.5d, 0.5d, 0.5d), colorGamut: colorGamut),
            new(new ScreenLightId(6, 3), "Bottom-Left", new LightPosition(-0.5d, -0.5d, -0.5d), colorGamut: colorGamut),
            new(new ScreenLightId(6, 4), "Bottom-Mid", new LightPosition(0, -0.5d, -0.5d), colorGamut: colorGamut),
            new(new ScreenLightId(6, 5), "Bottom-Right", new LightPosition(0.5d, -0.5d, -0.5d), colorGamut: colorGamut)
        };
    }

    private NDPLight[] GetLightsInternal()
    {
        ColorGamut colorGamut = Config.UseHDR ? ColorGamut.DisplayP3 : ColorGamut.sRGB;

        if (Config.LightCount == ScreenLightCount.Four)
            return GetLights4(colorGamut);
        else if (Config.LightCount == ScreenLightCount.Six)
            return GetLights6(colorGamut);
        else
            throw new InvalidLightHandlerConfigException("Invalid light count.");
    }

    public override async IAsyncEnumerable<NDPLight> GetLights()
    {
        foreach (NDPLight l in GetLightsInternal())
        {
            await Task.Delay(100);
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

    public override ValueTask<bool> Start(ErrorMessageCollector? errors, out NDPLight[] lights)
    {
        if (this.lights is null)
        {
            NDPLight[] l = GetLightsInternal();
            this.lights = new LightsContainer(l);
        }

        lights = this.lights.Lights;

        return new ValueTask<bool>(true);
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

    public override ValueTask Signal(LightId lightId, NDPColor color)
    {
        ScreenLightId light = (ScreenLightId)lightId;

        if (Signaler is null)
            throw new InvalidOperationException("Cannot signal without signaler.");

        int lightIndex = light.Index;
        int lightCount = light.TotalLightCount;

        const int rowCount = 2;
        int lightsPerRow = lightCount / rowCount;
        Debug.Assert((lightsPerRow * rowCount) == lightCount);

        (int yIndex, int xIndex) = Math.DivRem(light.Index, lightsPerRow);

        double x = xIndex / (double)(lightsPerRow - 1);
        double y = yIndex / (double)(rowCount - 1);
        Signaler.Signal(x, y, color);

        return ValueTask.CompletedTask;
    }
}
