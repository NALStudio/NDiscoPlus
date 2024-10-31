using NDiscoPlus.Shared.Effects.API.Channels.Effects.Intrinsics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.Shared.Models;
public sealed partial class ChunkedEffectsCollection
{
    // Use a wrapper class for debug effect visualizer so that we don't access these accidentally in ChunkedEffectsCollection itself
    // This must be thread-safe
    public readonly struct ExportedData
    {
        public string TrackId { get; }

        public ImmutableArray<Effect> Effects { get; }
        public ImmutableArray<NDPInterval> BackgroundDisabled { get; }

        public ExportedData(string trackId, ChunkedEffectsCollection source)
        {
            TrackId = trackId;

            Effects = source.effects;
            BackgroundDisabled = source.backgroundDisabled;
        }
    }
}
