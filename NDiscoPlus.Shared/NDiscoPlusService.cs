using NDiscoPlus.Shared.Analyzer;
using NDiscoPlus.Shared.Analyzer.Analysis;
using NDiscoPlus.Shared.Effects.API;
using NDiscoPlus.Shared.Effects.API.Channels.Background;
using NDiscoPlus.Shared.Effects.API.Channels.Effects;
using NDiscoPlus.Shared.Effects.API.Channels.Effects.Intrinsics;
using NDiscoPlus.Shared.Effects.BaseEffects;
using NDiscoPlus.Shared.Effects.Strobes;
using NDiscoPlus.Shared.Helpers;
using NDiscoPlus.Shared.Models;
using NDiscoPlus.Shared.Models.Color;
using NDiscoPlus.Shared.Music;
using NDiscoPlus.Spotify.Models;
using SkiaSharp;
using SpotifyAPI.Web;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace NDiscoPlus.Shared;

public class NDiscoPlusService
{
    // HttpClient should be thread-safe
    // Use a lazy-loaded global HttpClient to avoid socket exhaustion
    private static HttpClient? http = null;

    readonly Random random = new();

    private NDPColorPalette ModifyPaletteForEffects(NDiscoPlusArgs args, NDPColorPalette palette)
    {
        const int MinPaletteCount = 4;
        const int MaxPaletteCount = 5;

        // cull out similar colors
        List<NDPColor> newPalette = new(capacity: palette.Count);
        for (int i = 0; i < palette.Count; i++)
        {
            NDPColor col = palette[i];

            bool append = true;
            for (int j = 0; j < i; j++)
            {
                NDPColor c = palette[j];

                double xDist = col.X - c.X;
                double yDist = col.Y - c.Y;
                double distance = Math.Sqrt((xDist * xDist) + (yDist * yDist));
                if (distance < 0.1)
                    append = false;
            }

            if (append)
                newPalette.Add(col);
        }

        if (newPalette.Count < MinPaletteCount)
            return GetRandomDefaultPalette(args);

        if (newPalette.Count > MaxPaletteCount)
            return new NDPColorPalette(palette.Take(MaxPaletteCount));

        return palette;
    }

    public NDPData ComputeData(NDiscoPlusArgs args)
    {
        // *** ANALYSIS ***
        // Analyze audio
        AudioAnalysis analysis = AudioAnalyzer.Analyze(args.Features, args.Analysis);

        // Generate effects on analysis
        MusicEffectGenerator effectGen = MusicEffectGenerator.CreateRandom(random);
        GeneratedEffects effects = effectGen.Generate(analysis);


        // *** GENERATION ***
        NDPColorPalette referencePalette = args.ReferencePalette ?? GetRandomDefaultPalette(args);
        NDPColorPalette effectPalette = ModifyPaletteForEffects(args, referencePalette);

        EffectAPI api = new(
            args.Effects,
            args.Lights
        );

        BaseContext context = new(
            random: random,
            palette: effectPalette
        );

        // Background
        BackgroundContext backgroundContext = BackgroundContext.Extend(context, analysis);
        new ColorCycleBackgroundEffect().Generate(backgroundContext, api);

        // Strobes (before effects)
        StrobeContext strobeContext = StrobeContext.Extend(context, analysis, effects);
        foreach (NDPStrobe strobe in NDPStrobe.BeforeEffects)
            strobe.Generate(strobeContext, api);

        // Effects
        foreach (EffectRecord eff in effects.Effects)
        {
            EffectContext effectContext = EffectContext.Extend(context, eff.Section);
            eff.Effect?.Generate(effectContext, api);
        }

        // Strobes (after effects)
        foreach (NDPStrobe strobe in NDPStrobe.AfterEffects)
            strobe.Generate(strobeContext, api);

        float endOfFadeIn = args.Analysis.Track.EndOfFadeIn;
        float startOfFadeOut = args.Analysis.Track.StartOfFadeOut;

        ImmutableDictionary<LightId, NDPColor> lightPaletteColors = args.Lights.ToImmutableDictionary(
            keySelector: static lr => lr.Light.Id,
            elementSelector: _ => random.Choice(effectPalette)
        );

        // *** OUTPUT ***
        return new NDPData(
            track: args.Track,
            referencePalette: referencePalette,
            effectPalette: effectPalette,

            effectConfig: api.Config,
            effects: ChunkedEffectsCollection.Construct(api),

            lights: args.Lights,
            lightPaletteAssignedColors: lightPaletteColors
        );
    }

    private NDPColorPalette GetRandomDefaultPalette(NDiscoPlusArgs args)
        => NDPDefaultPalettes.GetRandomPalette(random, allowHDR: args.AllowHDR);

    public async Task<NDPColorPalette?> FetchImagePalette(SpotifyPlayerTrack track)
    {
        LazyLoadHttpClient();

        TrackImage largestImage = track.Images[0];
        int targetHalfSize = largestImage.Size / 2;
        // Try find an image closest to 50 % of the largest image available (to reduce the power required for palette computation)
        TrackImage halfSizeImage = track.Images.MinBy(img => Math.Abs(targetHalfSize - img.Size));

        var result = await http.GetAsync(halfSizeImage.Url);
        if (!result.IsSuccessStatusCode)
            return null;

        SKBitmap bitmap = SKBitmap.Decode(await result.Content.ReadAsStreamAsync());
        uint[] pixels = Array.ConvertAll(bitmap.Pixels, p => (uint)p);
        List<uint> rawColors = MaterialColorUtilities.Utils.ImageUtils.ColorsFromImage(pixels);

        return new NDPColorPalette(rawColors.Select(c => new SKColor(c)));
    }

    [MemberNotNull(nameof(http))]
    private static void LazyLoadHttpClient()
    {
        http ??= new();
    }
}