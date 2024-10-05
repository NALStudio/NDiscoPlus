using NDiscoPlus.Shared;
using NDiscoPlus.Shared.Analyzer;
using NDiscoPlus.Shared.Analyzer.Analysis;
using NDiscoPlus.Shared.Effects.Effects;
using NDiscoPlus.Shared.Models;
using SpotifyAPI.Web;
using System.Collections.Immutable;
using System.Diagnostics;

namespace NDiscoPlus.Shared.Music;

internal class EffectRecord
{
    public NDPEffect? Effect { get; }
    public AudioAnalysisSection Section { get; }
    // public NDPInterval Interval { get; }

    public EffectRecord(NDPEffect? effect, AudioAnalysisSection section)
    {
        Effect = effect;
        Section = section;
        // Interval = NDPInterval.FromSeconds(section.Start, section.Duration);
    }
}

internal class GeneratedEffects
{
    public ImmutableArray<EffectRecord> Effects { get; }

    public GeneratedEffects(ImmutableArray<EffectRecord> effects)
    {
        Effects = effects;
    }
}

public readonly record struct ComputedIntensity(EffectIntensity Intensity, AudioAnalysisSection Section);

public class MusicEffectGenerator
{
    private record struct IntensityComparison(double Loudness)
    {
        public static implicit operator IntensityComparison(AudioAnalysisSection section)
            => new(Loudness: section.Loudness);

        public static implicit operator IntensityComparison(AudioAnalysisFeatures features)
            => new(Loudness: features.Loudness);
    }

    internal IDictionary<EffectIntensity, NDPEffect?> Effects { get; }

    private static readonly int minIntensity = Enum.GetValues<EffectIntensity>().Min(i => (int)i);
    private static readonly int maxIntensity = Enum.GetValues<EffectIntensity>().Max(i => (int)i);

    internal MusicEffectGenerator(IEnumerable<KeyValuePair<EffectIntensity, NDPEffect?>> effects)
    {
        Effects = effects.ToImmutableDictionary();
    }

    /// <summary>
    /// Create a new music effect generator with a random configuration.
    /// </summary>
    internal static MusicEffectGenerator CreateRandom(Random random)
    {
        EffectIntensity[] effectIntensities = Enum.GetValues<EffectIntensity>();

        Dictionary<EffectIntensity, NDPEffect?> effects = effectIntensities
            .Select<EffectIntensity, KeyValuePair<EffectIntensity, NDPEffect?>>(i =>
            {
                IList<NDPEffect> effects = NDPEffect.ByIntensity[i];
                if (effects.Count > 0)
                    return new(i, effects[random.Next(effects.Count)]);
                else
                    return new(i, null);
            })
            .ToDictionary();

        return new MusicEffectGenerator(effects);
    }

    private static bool SectionIsEmpty(AudioAnalysisSection s)
    {
        AudioAnalysisTimings t = s.Timings;
        return t.Bars.IsEmpty && t.Beats.IsEmpty && t.Tatums.IsEmpty;
    }

    internal GeneratedEffects Generate(AudioAnalysis analysis)
    {
        ImmutableArray<EffectRecord> effects = InternalGenerate(analysis).ToImmutableArray();
        return new(effects);
    }

    private IEnumerable<EffectRecord> InternalGenerate(AudioAnalysis analysis)
    {
        foreach ((EffectIntensity intensity, AudioAnalysisSection section) in ComputeIntensities(analysis))
        {
            NDPEffect? effect = null;
            if (Effects.TryGetValue(intensity, out NDPEffect? vDefault))
                effect ??= vDefault;
            if (Effects.TryGetValue(intensity - 1, out NDPEffect? vLower))
                effect ??= vLower;
            if (Effects.TryGetValue(intensity + 1, out NDPEffect? vHigher))
                effect ??= vHigher;

            // tempo < 1 is unrealistic, either there aren't any beats or they don't make any sense.
            // if tempo of 0 is handled correctly, animations happen in weird places where they don't make sense
            // and if it is not handled correctly, the application crashes
            // in either case, we don't want to generate any effects when tempo is 0.
            if (section.Tempo < 1f)
                effect = null;
            yield return new EffectRecord(effect, section);
        }
    }

    public static List<ComputedIntensity> DebugComputeIntensities(TrackAudioFeatures features, TrackAudioAnalysis analysis)
        => ComputeIntensities(AudioAnalyzer.Analyze(features, analysis));
    internal static List<ComputedIntensity> ComputeIntensities(AudioAnalysis analysis)
    {
        int? previousIntensity = null;
        bool doubleJumped = false;

        AudioAnalysisSection? loudestSection = analysis.Sections.MaxBy(s => s.Loudness);

        List<ComputedIntensity> values = new();

        for (int i = 0; i < analysis.Sections.Length; i++)
        {
            AudioAnalysisSection section = analysis.Sections[i];
            bool isLastSection = i >= (analysis.Sections.Length - 1);

            AudioAnalysisSection? previousSection = i > 0 ? analysis.Sections[i - 1] : null;

            int intensity;
            if (previousIntensity is int pIntensity)
            {
                Debug.Assert(previousSection is not null);
                intensity = pIntensity;

                int intensityComparison = CompareIntensities(section, previousSection);
                if (intensityComparison >= 0)
                    intensity++;
                else
                    intensity--;

                if (doubleJumped && !isLastSection)
                    intensity--;

                if (ReferenceEquals(section, loudestSection) && intensity < maxIntensity)
                {
                    intensity++;
                    doubleJumped = true;
                }
                else
                {
                    doubleJumped = false;
                }

                if (intensity > maxIntensity && !doubleJumped)
                {
                    intensity--;
                    AdjustDown(values);
                }

                if (intensity > maxIntensity)
                    intensity = maxIntensity;
                if (intensity < minIntensity)
                    intensity = minIntensity;
            }
            else
            {
                Debug.Assert(previousSection is null);

                intensity = (int)IntensityFromFeatures(analysis.Features);

                // if section is less intensive than features, decrement intensity if possible
                // we do this to be extra conservative with the first section's intensity
                // as it determines the rest of the song's intensity and usually is one of the calmer parts of the song.
                if (CompareIntensities(section, analysis.Features) < 0)
                    intensity--;
                if (section.Loudness < -15)
                    intensity--;

                if (intensity < minIntensity)
                    intensity = minIntensity;
                Debug.Assert(intensity <= maxIntensity);
            }

            Debug.Assert(intensity >= minIntensity, "Intensity must be verified and adjusted inside the if statement.");
            Debug.Assert(intensity <= maxIntensity, "Intensity must be verified and adjusted inside the if statement.");
            values.Add(new ComputedIntensity((EffectIntensity)intensity, section));
            previousIntensity = intensity;
        }

        return values;
    }

    private static void AdjustDown(List<ComputedIntensity> values)
    {
        for (int i = 0; i < values.Count; i++)
        {
            ComputedIntensity computed = values[i];

            int intensity = (int)computed.Intensity;
            intensity--;
            if (intensity < minIntensity)
                intensity = minIntensity;

            values[i] = computed with { Intensity = (EffectIntensity)intensity };
        }
    }

    /// <summary>
    /// Compute an intensity value (in the range of -1 to 1) for a in relation to b.
    /// </summary>
    private static int CompareIntensities(IntensityComparison a, IntensityComparison b)
    {
        // Only compare loudness for now
        // as comparing tempo etc. made this method unreliable.
        return a.Loudness.CompareTo(b.Loudness);
    }

    public static EffectIntensity DebugIntensityFromFeatures(TrackAudioFeatures features)
        => IntensityFromFeatures(AudioAnalysisFeatures.FromSpotify(features));
    internal static EffectIntensity IntensityFromFeatures(AudioAnalysisFeatures features)
    {
        // range: 0 - 4 where 4 is very rare
        int intensityRef = (int)(features.Energy * 4f);

        // range: 1 - 5 where 5 is very rare
        int intensity = intensityRef + 1;

        // EffectIntensity.VeryHigh is very rare
        return (EffectIntensity)intensity;
    }
}
