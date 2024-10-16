using MudBlazor;
using NDiscoPlus.Components.Elements;
using NDiscoPlus.Components.Elements.GradientCanvas;
using NDiscoPlus.Shared.Models;
using NDiscoPlus.Shared.Models.Color;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace NDiscoPlus.Code.LightHandlers.Screen;

internal readonly record struct ScreenLight(NDPLight Light, NDPColor? Color)
{
    public ScreenLight ReplaceColor(NDPColor color)
        => new(Light, color);
}

internal class ScreenLightHandler : BaseScreenLightHandler
{
    public ScreenLightSignaler? Signaler { get; set; }

    public ScreenLightHandler(ScreenLightHandlerConfig? config) : base(config)
    {
    }

    private new ScreenLightHandlerConfig Config => (ScreenLightHandlerConfig)base.Config;
    protected override LightHandlerConfig CreateConfig()
        => new ScreenLightHandlerConfig();

    private NDPLight[] GetLights4()
    {
        ScreenLightMetrics metrics = Config.LightMetrics;

        return new NDPLight[]
        {
            CreateLight(new ScreenLightId(4, 0), new LightPosition(metrics.Left,  metrics.Y, metrics.Top)),
            CreateLight(new ScreenLightId(4, 1), new LightPosition(metrics.Right, metrics.Y, metrics.Top)),
            CreateLight(new ScreenLightId(4, 2), new LightPosition(metrics.Left,  metrics.Y, metrics.Bottom)),
            CreateLight(new ScreenLightId(4, 3), new LightPosition(metrics.Right, metrics.Y, metrics.Bottom))
        };
    }

    private NDPLight[] GetLights6()
    {
        ScreenLightMetrics metrics = Config.LightMetrics;

        return new NDPLight[]
        {
            CreateLight(new ScreenLightId(6, 0), new LightPosition(metrics.Left,  metrics.Y, metrics.Top)),
            CreateLight(new ScreenLightId(6, 1), new LightPosition(metrics.Mid,   metrics.Y, metrics.Top)),
            CreateLight(new ScreenLightId(6, 2), new LightPosition(metrics.Right, metrics.Y, metrics.Top)),
            CreateLight(new ScreenLightId(6, 3), new LightPosition(metrics.Left,  metrics.Y, metrics.Bottom)),
            CreateLight(new ScreenLightId(6, 4), new LightPosition(metrics.Mid,   metrics.Y, metrics.Bottom)),
            CreateLight(new ScreenLightId(6, 5), new LightPosition(metrics.Right, metrics.Y, metrics.Bottom))
        };
    }

    private NDPLight CreateLight(ScreenLightId id, LightPosition pos)
    {
        ColorGamut colorGamut = Config.ColorGamut;

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
        bool valid = Config.ValidateConfig(errors);

        if (!Enum.GetValues<ScreenLightCount>().Contains(Config.LightCount))
        {
            errors?.Add($"Invalid Screen Light Count: {Config.LightCount}");
            valid = false;
        }

        return new ValueTask<bool>(valid);
    }

    public override ValueTask<NDPLight[]?> Start(ErrorMessageCollector? errors)
    {
        NDPLight[] l = GetLightsInternal();
        return new(l);
    }

    protected override RenderMeta? RenderUpdate(LightColorCollection lightColors)
    {
        NDPColor[] colors = new NDPColor[(int)Config.LightCount];

        foreach ((ScreenLightId light, NDPColor color) in lightColors.OfType<ScreenLightId>()) // get lights of correct type
            colors[light.Index] = color;

        return new(typeof(CornerGradientCanvas), ImmutableCollectionsMarshal.AsImmutableArray(colors));
    }

    public override ValueTask Stop()
    {
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
