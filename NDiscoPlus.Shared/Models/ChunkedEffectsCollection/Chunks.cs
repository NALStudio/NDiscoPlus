using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.Shared.Models;
public sealed partial class ChunkedEffectsCollection
{
    internal readonly struct Chunk
    {
        private readonly List<int> effectIndexes;
        private readonly ReadOnlyCollection<int> effectIndexesReadonly;

        private readonly List<int> backgroundDisabledIndexes;
        private readonly ReadOnlyCollection<int> backgroundDisabledIndexesReadonly;

        public IEnumerable<int> EffectIndexes => effectIndexesReadonly;
        public IEnumerable<int> BackgroundDisabledIndexes => backgroundDisabledIndexesReadonly;

        public void AddEffect(int effectIndex)
            => effectIndexes.Add(effectIndex);
        public void AddBackgroundDisabled(int disabledIntervalIndex)
            => backgroundDisabledIndexes.Add(disabledIntervalIndex);

        public Chunk()
        {
            effectIndexes = new();
            effectIndexesReadonly = effectIndexes.AsReadOnly();

            backgroundDisabledIndexes = new();
            backgroundDisabledIndexesReadonly = backgroundDisabledIndexes.AsReadOnly();
        }
    }

    private class ChunkBuilder
    {
        private readonly List<Chunk> chunks = new();

        public void ExtendToIndex(int index)
        {
            while (chunks.Count <= index)
                chunks.Add(new Chunk());
        }

        public Chunk At(int index) => chunks[index];

        public ImmutableArray<Chunk> Build() => chunks.ToImmutableArray();
    }
}
