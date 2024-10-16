using NDiscoPlus.Components.Elements.GradientCanvas;
using NDiscoPlus.Shared.Helpers;
using NDiscoPlus.Shared.Models;
using NDiscoPlus.Shared.Models.Color;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ColorPosition = NDiscoPlus.Components.Elements.GradientCanvas.HorizontalGradientCanvas.ColorPosition;

namespace NDiscoPlus.Code.LightHandlers.Screen;
internal class ScreenMimicLightHandler : BaseScreenLightHandler
{
    private FrozenDictionary<LightId, LightPosition>? lightPositions;

    public ScreenMimicLightHandler(ScreenMimicLightHandlerConfig? config) : base(config)
    {
    }

    public new ScreenMimicLightHandlerConfig Config => (ScreenMimicLightHandlerConfig)base.Config;
    protected override LightHandlerConfig CreateConfig()
        => new ScreenMimicLightHandlerConfig();

    public override async IAsyncEnumerable<NDPLight> GetLights()
    {
        // Minimum required code to make an empty IAsyncEnumerable lol
        await Task.Delay(50);
        yield break;
    }

    public override ValueTask<TimeSpan?> Signal(LightId lightId, NDPColor color)
    {
        throw new InvalidOperationException("How can you signal if this light handler doesn't provide any lights?");
    }

    public override ValueTask<NDPLight[]?> Start(ErrorMessageCollector? errors)
    {
        return new(Array.Empty<NDPLight>());
    }
    public override ValueTask OnAfterStart(ImmutableArray<LightRecord> allLights)
    {
        // Run in separate thread to not block the UI thread
        // most likely doesn't make any difference whatsoever but I want to be pre-emptive
        // rather than discover later that the reason my UI locks up randomly is due to this
        lightPositions = BuildLightDictionary(allLights, Config.LightMetrics).ToFrozenDictionary();
        return ValueTask.CompletedTask;
    }

    private static IEnumerable<KeyValuePair<LightId, LightPosition>> BuildLightDictionary(ImmutableArray<LightRecord> lights, ScreenLightMetrics metrics)
    {
        double top = metrics.Top;
        double bottom = metrics.Bottom;
        double left = metrics.Left;
        double right = metrics.Right;
        double y = metrics.Y;

        foreach (LightRecord lr in lights)
        {
            LightPosition lightPos = lr.Light.Position;
            if (lightPos.Z > top || lightPos.Z < bottom) // light must be vertically inside screen (edges count as 'inside')
                continue;
            if (lightPos.X < left || lightPos.X > right) // light must be horizontally inside screen (edges count as 'inside')
                continue;
            if (lightPos.Y < y) // light must be behind screen (light position front-back can equal screen position and is counted as 'behind')
                continue;

            yield return new(lr.Light.Id, lightPos);
        }
    }

    public override ValueTask Stop()
    {
        lightPositions = null;
        return ValueTask.CompletedTask;
    }

    public override ValueTask<bool> ValidateConfig(ErrorMessageCollector? errors)
    {
        bool valid = Config.ValidateConfig(errors);

        if (Config.BrightnessMultiplier < 0d || Config.BrightnessMultiplier > 1d)
        {
            errors?.Add("Brightness multiplier value must in range [0, 1]");
            valid = false;
        }

        return new(valid);
    }

    protected override RenderMeta? RenderUpdate(LightColorCollection lights)
    {
        if (lightPositions is null)
            throw new InvalidOperationException("ScreenMimicLightHandler was not started! (OnAfterStart not called)");

        double left = Config.LightMetrics.Left;
        double right = Config.LightMetrics.Right;

        ColorGamut colorGamut = Config.ColorGamut;
        double brightnessMultiplier = Config.BrightnessMultiplier;

        // Keep track of count manually instead of using a list
        // so that we can use ImmutableCollectionsMarshal to turn this into an immutable array
        ColorPosition[] colors = new ColorPosition[lightPositions.Count];
        int colorsCount = 0;
        foreach (KeyValuePair<LightId, NDPColor> l in lights)
        {
            if (!lightPositions.TryGetValue(l.Key, out LightPosition lightPos))
                continue;

            // light and screen positions are from -1 to 1
            // remap light onto screen where the gradient color position is from 0 to 1
            double pos = lightPos.X.Remap(left, right, 0, 1);

            NDPColor color = l.Value;
            if (brightnessMultiplier != 1d)
                color = new NDPColor(color.X, color.Y, color.Brightness * brightnessMultiplier);
            color = color.Clamp(colorGamut);

            colors[colorsCount] = new ColorPosition(pos, color);
            colorsCount++;
        }

        Debug.Assert(colorsCount == colors.Length, "Some lights were missing color data.");

        if (colors.Length < 1)
            return null; // Horizontal gradient canvas will crash if colors.Count < 1

        Type gradientCanvasType = Config.Variant switch
        {
            ScreenHorizontalLightsVariant.Continuous => typeof(HorizontalGradientCanvas),
            ScreenHorizontalLightsVariant.FixedWidth => typeof(FixedWidthHorizontalGradientCanvas),
            var v => throw new InvalidOperationException($"Invalid variant: {v}")
        };
        return new RenderMeta(gradientCanvasType, ImmutableCollectionsMarshal.AsImmutableArray(colors));
    }
}
