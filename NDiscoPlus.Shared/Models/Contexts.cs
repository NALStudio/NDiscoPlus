using NDiscoPlus.Shared.Analyzer.Analysis;
using NDiscoPlus.Shared.Music;
using SpotifyAPI.Web;
using System.Collections.Immutable;

namespace NDiscoPlus.Shared.Models;

internal class Context
{
    public Random Random { get; }
    public NDPColorPalette Palette { get; }

    public AudioAnalysis Analysis { get; }

    public Context(Random random, NDPColorPalette palette, AudioAnalysis analysis)
    {
        Random = random;
        Palette = palette;
        Analysis = analysis;
    }
}

internal sealed class StrobeContext : Context
{
    public GeneratedEffects Effects { get; }

    private StrobeContext(Random random, NDPColorPalette palette, AudioAnalysis analysis, GeneratedEffects effects) : base(random, palette, analysis)
    {
        Effects = effects;
    }

    public static StrobeContext Extend(Context context, GeneratedEffects effects)
    {
        return new StrobeContext(
            random: context.Random,
            palette: context.Palette,
            analysis: context.Analysis,
            effects: effects
        );
    }

    public bool IntensityAtLeast(EffectIntensity intensity, NDPInterval interval)
    {
        IEnumerable<EffectRecord> overlappingEffects = Effects.Effects.Where(effect => NDPInterval.Overlap(effect.Section.Interval, interval));

        // returns true if enumerable is empty
        return overlappingEffects.All(effect => effect.Effect is null || effect.Effect.Intensity >= intensity);
    }
}

internal sealed class EffectContext : Context
{
    public AudioAnalysisSection Section { get; }

    private EffectContext(Random random, NDPColorPalette palette, AudioAnalysis analysis, AudioAnalysisSection section) : base(random, palette, analysis)
    {
        Section = section;
    }

    public static EffectContext Extend(Context context, AudioAnalysisSection section)
    {
        return new EffectContext(
            random: context.Random,
            palette: context.Palette,
            analysis: context.Analysis,
            section: section
        );
    }
}