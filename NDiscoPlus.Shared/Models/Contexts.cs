using NDiscoPlus.Shared.Analyzer.Analysis;
using NDiscoPlus.Shared.Music;

namespace NDiscoPlus.Shared.Models;

internal class BaseContext
{
    public Random Random { get; }
    public NDPColorPalette Palette { get; }

    public BaseContext(Random random, NDPColorPalette palette)
    {
        Random = random;
        Palette = palette;
    }
}

internal abstract class WholeTrackContext : BaseContext
{
    public AudioAnalysis Analysis { get; }

    protected WholeTrackContext(Random random, NDPColorPalette palette, AudioAnalysis analysis) : base(random, palette)
    {
        Analysis = analysis;
    }
}

internal sealed class BackgroundContext : WholeTrackContext
{
    public BackgroundContext(Random random, NDPColorPalette palette, AudioAnalysis analysis) : base(random, palette, analysis)
    {
    }

    public static BackgroundContext Extend(BaseContext context, AudioAnalysis analysis)
    {
        return new BackgroundContext(
            random: context.Random,
            palette: context.Palette,
            analysis: analysis
        );
    }
}

internal sealed class StrobeContext : WholeTrackContext
{
    public GeneratedEffects Effects { get; }

    private StrobeContext(Random random, NDPColorPalette palette, AudioAnalysis analysis, GeneratedEffects effects) : base(random, palette, analysis)
    {
        Effects = effects;
    }

    public static StrobeContext Extend(BaseContext context, AudioAnalysis analysis, GeneratedEffects effects)
    {
        return new StrobeContext(
            random: context.Random,
            palette: context.Palette,
            analysis: analysis,
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

internal sealed class EffectContext : BaseContext
{
    public AudioAnalysisSection Section { get; }

    private EffectContext(Random random, NDPColorPalette palette, AudioAnalysisSection section) : base(random, palette)
    {
        Section = section;
    }

    public static EffectContext Extend(BaseContext context, AudioAnalysisSection section)
    {
        return new EffectContext(
            random: context.Random,
            palette: context.Palette,
            section: section
        );
    }
}