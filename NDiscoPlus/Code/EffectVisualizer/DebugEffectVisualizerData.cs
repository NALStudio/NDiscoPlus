using NDiscoPlus.Shared.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Effect = NDiscoPlus.Shared.Effects.API.Channels.Effects.Intrinsics.Effect;

namespace NDiscoPlus.Code.EffectVisualizer;
public readonly record struct DebugEffectVisualizerData(
    LightId Light,
    ImmutableArray<Effect> Effects,
    TimeSpan TrackLength
);
