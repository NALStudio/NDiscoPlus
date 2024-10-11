using NDiscoPlus.Shared.Effects.API;
using NDiscoPlus.Shared.Effects.API.Channels.Background;
using NDiscoPlus.Shared.Models;

namespace NDiscoPlus.Shared.Effects.BaseEffects;

internal abstract class NDPBackgroundEffect
{
    protected NDPBackgroundEffect()
    {
    }

    public abstract void Generate(BackgroundContext ctx, EffectAPI api);
}