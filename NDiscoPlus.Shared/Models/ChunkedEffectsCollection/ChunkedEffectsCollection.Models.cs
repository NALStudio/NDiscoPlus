using NDiscoPlus.Shared.Effects.API.Channels.Background.Intrinsics;
using NDiscoPlus.Shared.Effects.API.Channels.Effects.Intrinsics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.Shared.Models;
public sealed partial class ChunkedEffectsCollection
{
    internal class Chunk
    {
        private readonly List<int> effectIndexes;
        private readonly List<int> backgroundTransitionIndexes;

        public IEnumerable<int> EffectIndexes => effectIndexes.AsReadOnly();
        public IEnumerable<int> BackgroundTransitionIndexes => backgroundTransitionIndexes.AsReadOnly();

        public NDPInterval? Disabled { get; private set; }

        public void AddEffect(int effectIndex)
            => effectIndexes.Add(effectIndex);

        public void AddBackgroundTransition(int transitionIndex)
            => backgroundTransitionIndexes.Add(transitionIndex);

        public void SetDisabled(NDPInterval disabled)
            => Disabled = disabled;


        public Chunk()
        {
            effectIndexes = new();
            backgroundTransitionIndexes = new();
        }
    }

    public readonly struct BackgroundDisabled
    {
        [MemberNotNullWhen(true, nameof(DisabledInterval))]
        public bool IsDisabled { get; }
        public NDPInterval? DisabledInterval { get; }

        internal BackgroundDisabled(NDPInterval? disabledInterval)
        {
            IsDisabled = disabledInterval is not null;
            DisabledInterval = disabledInterval;
        }
    }

    internal sealed class ChunkView
    {
        private readonly Chunk? chunk;
        private readonly ChunkedEffectsCollection parent;

        internal ChunkView(Chunk? chunk, ChunkedEffectsCollection parent)
        {
            this.chunk = chunk;
            this.parent = parent;
        }

        public IEnumerable<Effect> GetEffects()
        {
            if (chunk is null)
                yield break;

            foreach (int index in chunk.EffectIndexes)
                yield return parent.effects[index];
        }

        public IEnumerable<BackgroundTransition> GetBackgroundTransitions()
        {
            if (chunk is null)
                yield break;

            foreach (int index in chunk.BackgroundTransitionIndexes)
                yield return parent.backgroundTransitions[index];
        }

        public BackgroundDisabled IsBackgroundDisabled => new(chunk?.Disabled);
    }
}
