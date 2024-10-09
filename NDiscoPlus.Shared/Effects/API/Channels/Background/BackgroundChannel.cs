using NDiscoPlus.Shared.Effects.API.Channels.Background.Intrinsics;
using NDiscoPlus.Shared.Effects.API.Channels.Effects.Intrinsics;
using NDiscoPlus.Shared.Helpers;
using NDiscoPlus.Shared.Models;
using System.Collections.Immutable;

namespace NDiscoPlus.Shared.Effects.API.Channels.Background;

/// <summary>
/// This class is used by effects to gather information on what the background is currently doing.
/// </summary>
public class BackgroundChannel : BaseChannel
{
    public BackgroundChannel(NDPLightCollection lights) : base(lights)
    {
    }

    protected readonly List<BackgroundTransition> transitions = new();
    protected readonly List<NDPInterval> disabled = new();

    public IList<BackgroundTransition> Transitions => transitions.AsReadOnly();
    public IList<NDPInterval> DisabledIntervals => disabled.AsReadOnly();

    public void DisableFor(NDPInterval interval)
    {
        disabled.Add(interval);
    }

    /// <summary>
    /// <para>Get the currently running transition for the given light.</para>
    /// <para>If not transitions are running, returns the next transition scheduled.</para>
    /// <para>If there are no transitions whatsoever or the light doesn't exist, returns null.</para>
    /// </summary>
    public BackgroundTransition? GetAt(LightId light, TimeSpan position)
    {
        if (!transitions.TryGetValue(light, out List<BackgroundTransition>? trans))
            return null;

        // trans[..index].Start <= position
        // trans[index..].Start > position
        int index = Bisect.BisectRight(trans, position, t => t.Start);
        if (index > 0 && trans[index - 1].End > position)
            return trans[index - 1];
        else if (trans.Count > 0)
            return trans[index];
        else
            return null;
    }
}

/// <summary>
/// This class is used by background effect generators to generate background effects.
/// </summary>
public class BackgroundChannelAPI : BackgroundChannel
{
    public EffectConfig Config { get; }

    public BackgroundChannelAPI(NDPLightCollection lights, EffectConfig config) : base(lights)
    {
        Config = config;
    }

    public void Add(BackgroundTransition transition)
    {
        if (!transitions.TryGetValue(transition.LightId, out List<BackgroundTransition>? trans))
        {
            trans = new();
            transitions.Add(transition.LightId, trans);
        }

        Bisect.InsortRight(trans, transition, t => t.Start);
    }
}