using NDiscoPlus.Shared.Effects.API.Channels.Effects.Intrinsics;
using NDiscoPlus.Shared.Helpers;
using NDiscoPlus.Shared.Models;
using NDiscoPlus.Shared.Models.Color;
using System.Text.Json.Serialization;

namespace NDiscoPlus.Shared.Effects.API.Channels.Effects;

public class EffectChannel : BaseChannel
{
    public Channel Channel { get; }

    public EffectChannel(Channel channel, IEnumerable<NDPLight> lights) : base(lights)
    {
        Channel = channel;
    }

    public IList<Effect> Effects => effects.AsReadOnly();
    private readonly List<Effect> effects = new();

    public void Add(Effect effect)
        => Bisect.InsortRight(effects, effect, e => e.Position);

    /// <summary>
    /// <para>
    /// Clear all effects that are contained within the given range.
    /// If you want to also remove values that clip with the range (start outside and end inside, or vice versa)
    /// use <see cref="Purge"/> instead.
    /// </para>
    /// <para>Start is inclusive, end is exclusive.</para>
    /// </summary>
    public void Clear(TimeSpan start, TimeSpan end)
        => effects.RemoveAll(e => e.Start >= start && e.End < end);

    /// <summary>
    /// <para>
    /// Clear all effects that clip with the given range.
    /// If you want to only remove values that are contained within the range (start and end inside)
    /// use <see cref="Clear"/> instead.
    /// </para>
    /// <para>Start is inclusive, end is exclusive.</para>
    /// </summary>
    public void Purge(TimeSpan start, TimeSpan end)
        => effects.RemoveAll(e => e.End >= start && e.Start < end);

    /// <summary>
    /// Get all the effects that are running at the given <paramref name="position"/>.
    /// </summary>
    /// <remarks>
    /// Both ends of <see cref="Effect"/> are inclusive.
    /// </remarks>
    public IEnumerable<Effect> GetBusyEffects(TimeSpan position)
        => effects.Where(e => e.End >= position && e.Start <= position);

    /// <summary>
    /// Get all the effects that are running within the given <paramref name="interval"/>.
    /// </summary>
    /// <remarks>
    /// Both ends of <see cref="Effect"/> and <see cref="NDPInterval"/> are inclusive.
    /// </remarks>
    public IEnumerable<Effect> GetBusyEffects(NDPInterval interval)
        => effects.Where(e => e.Start <= interval.End && e.End >= interval.Start);

    public IEnumerable<NDPLight> GetBusyLights(TimeSpan position)
        => GetBusyLightsInternal(position).Select(id => Lights[id]);

    private HashSet<LightId> GetBusyLightsInternal(TimeSpan position)
    {
        HashSet<LightId> ids = new();
        foreach (Effect eff in GetBusyEffects(position))
            _ = ids.Add(eff.LightId);
        return ids;
    }

    public IEnumerable<NDPLight> GetAvailableLights(TimeSpan position)
    {
        HashSet<LightId> reserved = GetBusyLightsInternal(position);
        foreach (NDPLight light in Lights.Values)
        {
            if (!reserved.Contains(light.Id))
                yield return light;
        }
    }
}
