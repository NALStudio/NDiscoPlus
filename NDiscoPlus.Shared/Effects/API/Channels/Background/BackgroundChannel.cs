using NDiscoPlus.Shared.Effects.API.Channels.Background.Intrinsics;
using NDiscoPlus.Shared.Effects.API.Channels.Effects.Intrinsics;
using NDiscoPlus.Shared.Helpers;
using NDiscoPlus.Shared.Models;
using System.Collections.Immutable;

namespace NDiscoPlus.Shared.Effects.API.Channels.Background;

public sealed class BackgroundChannel : BaseChannel
{
    public BackgroundChannel(NDPLightCollection lights) : base(lights)
    {
    }

    private readonly List<BackgroundTransition> transitionsUnordered = new();
    private readonly List<NDPInterval> disabled = new();

    public IList<BackgroundTransition> TransitionsUnordered => transitionsUnordered.AsReadOnly();
    public IList<NDPInterval> DisabledIntervals => disabled.AsReadOnly();

    /// <summary>
    /// <para>This method should only be used by background effect generators, not regular effect generators.</para>
    /// <para>Due to code complexity issues, it has been left here for everyone's access.</para>
    /// </summary>
    public void Add(BackgroundTransition transition)
    {
        transitionsUnordered.Add(transition);
    }

    public void DisableFor(NDPInterval interval)
    {
        disabled.Add(interval);
    }
}