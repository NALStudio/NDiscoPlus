using NDiscoPlus.Shared.Effects.API.Channels.Background;
using NDiscoPlus.Shared.Effects.API.Channels.Background.Intrinsics;
using NDiscoPlus.Shared.Effects.API.Channels.Effects.Intrinsics;
using NDiscoPlus.Shared.Helpers;
using NDiscoPlus.Shared.Models;
using NDiscoPlus.Shared.Models.Color;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace NDiscoPlus.Shared;

/// <summary>
/// Thread-safe interpreter result.
/// </summary>
public readonly record struct LightInterpreterResult
{
    public LightInterpreterResult(LightColorCollection lights, double frameTime)
    {
        Lights = lights;
        FrameTime = frameTime;
    }

    public LightColorCollection Lights { get; }

    public double FrameTime { get; }
    public double FPS => 1d / FrameTime;
}

public class LightInterpreter
{
    private const int MAX_SIMULTANEOUS_ANIMATIONS = 64;
    private const int SIMULTANEOUS_ANIMATIONS_HALF_WINDOWSIZE = MAX_SIMULTANEOUS_ANIMATIONS / 2;

    private readonly Random random = new();
    private Stopwatch? deltaTimeSW;

    private static IEnumerable<(LightId Light, NDPColor Color)> UpdateBackground(TimeSpan progress, NDPData data)
    {
        int index = -1;
        foreach ((LightId lightId, ImmutableArray<BackgroundTransition> transitions) in data.Effects.BackgroundTransitions)
        {
            index++;

            int bIndex = Bisect.BisectRight(transitions, progress, t => t.Start);
            // transitions[bIndex].Start > progress
            // transitions[bIndex - 1].Start <= progress
            int currentIndex = bIndex - 1;
            // transitions[currentIndex].Start <= progress

            NDPColor paletteColor = data.EffectPalette[index % data.EffectPalette.Count];
            paletteColor = paletteColor.CopyWith(brightness: data.EffectConfig.BaseBrightness);

            NDPColor color;
            if (currentIndex > -1)
            {
                BackgroundTransition current = transitions[currentIndex];

                NDPColor prevColor;
                if (currentIndex > 0)
                {
                    BackgroundTransition prev = transitions[currentIndex - 1];
                    if (prev.End > current.Start)
                        throw new InvalidOperationException("Cannot run multiple background transitions on the same light simultaneously.");
                    prevColor = prev.Color;
                }
                else
                {
                    prevColor = paletteColor;
                }

                Debug.Assert(current.Start <= progress);

                color = current.Interpolate(progress, prevColor);
            }
            else
            {
                color = paletteColor;
            }

            yield return (lightId, color);
        }
    }

    private static void UpdateEffects(ref Dictionary<LightId, NDPColor> lights, NDPData data, TimeSpan progress)
    {
        foreach (Effect e in data.Effects.GetEffects(progress))
        {
            if (!lights.TryGetValue(e.LightId, out NDPColor oldColor))
            {
                // if no previous color found, use effect color
                // if effect doesn't have color, use strobe color
                oldColor = e.GetColor(data.EffectConfig.StrobeColor)
                            .CopyWith(brightness: 0d);
            }

            lights[e.LightId] = e.Interpolate(progress, oldColor);
        }
    }

    private static void HandleLimitationsAndAddMissingLights(ref Dictionary<LightId, NDPColor> lights, IEnumerable<LightRecord> allLights)
    {
        // supply color values for all lights
        foreach (LightRecord lightRecord in allLights)
        {
            NDPLight light = lightRecord.Light;

            if (lights.TryGetValue(light.Id, out NDPColor color))
            {
                // if lights exists, handle light limitations

                // clamp color to gamut (if gamut is available)
                if (light.ColorGamut is not null)
                    color = color.Clamp(light.ColorGamut);

                // handle brightness
                if (lightRecord.Brightness != 1d)
                    color = color.CopyWith(brightness: color.Brightness * lightRecord.Brightness);

                lights[light.Id] = color;
            }
            else
            {
                // if light doesn't exist, create a black for it (must be inside its color gamut so we use the color gamut's red XY position.)
                // we supply a default black value so that the consumer of this interpreter doesn't need to assign default colors itself.
                NDPColor defaultBlack;
                if (light.ColorGamut is not null)
                    defaultBlack = light.ColorGamut.Red.ToColor(brightness: 0d);
                else
                    defaultBlack = new NDPColor();

                lights[light.Id] = defaultBlack;
            }
        }
    }

    double TickDeltaTime()
    {
        if (deltaTimeSW is not null)
        {
            double deltaTime = deltaTimeSW.Elapsed.TotalSeconds;
            deltaTimeSW.Restart();
            return deltaTime;
        }
        else
        {
            deltaTimeSW = Stopwatch.StartNew();
            return 0d;
        }
    }

    public LightInterpreterResult Update(TimeSpan progress, NDPData data)
    {
        Dictionary<LightId, NDPColor> lights = UpdateBackground(progress, data).ToDictionary(key => key.Light, value => value.Color);

        UpdateEffects(ref lights, data, progress);
        HandleLimitationsAndAddMissingLights(ref lights, data.Lights);

        double deltaTime = TickDeltaTime();

        return new LightInterpreterResult(
            lights: LightColorCollection.UnsafeRef(lights), // should be (thread-)safe as we don't keep a reference to the dictionary after this function ends
            frameTime: deltaTime
        );
    }
}
