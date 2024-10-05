using NDiscoPlus.Shared.Effects.API.Channels.Effects;
using NDiscoPlus.Shared.Helpers;
using NDiscoPlus.Shared.Models;
using System.Collections.Immutable;
using System.Diagnostics;

namespace NDiscoPlus.Shared.Effects.Effects.Strobes;
internal class GroupedStrobeLightEffect : BaseStrobeLightEffect
{
    public enum GroupingType { Horizontal, Vertical, RandomPattern }

    public GroupedStrobeLightEffect(GroupingType grouping, EffectIntensity intensity) : base(intensity)
    {
        Grouping = grouping;
    }

    public GroupingType Grouping { get; }

    protected override IEnumerable<LightGroup> Group(EffectContext ctx, NDPLightCollection lights, int frameCount, int groupCount)
    {
        if (groupCount > lights.Count)
        {
            // is divisible by 2
            if (groupCount % 2 == 0)
                groupCount /= 2;
            else
                groupCount = (groupCount / 2) + 1; // ceil divide
        }

        List<NDPLight[]> groups = Grouping switch
        {
            GroupingType.Horizontal => lights.GroupX(groupCount),
            GroupingType.Vertical => lights.GroupZ(groupCount),
            GroupingType.RandomPattern => ctx.Random.Group(lights.Values, groupCount).ToList(),
            _ => throw new NotImplementedException()
        };
        Debug.Assert(groups.Count == groupCount);

        return groups.Select(static group => LightGroup.FromLights(group));
    }
}
