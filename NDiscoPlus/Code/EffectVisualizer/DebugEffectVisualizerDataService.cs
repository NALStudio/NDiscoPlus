using NDiscoPlus.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.Code.EffectVisualizer;
public class DebugEffectVisualizerDataService
{
    private readonly object dataLock = new();
    private ChunkedEffectsCollection.ExportedData? data;

    public ChunkedEffectsCollection.ExportedData GetData()
    {
        ChunkedEffectsCollection.ExportedData? data;
        lock (dataLock)
        {
            data = this.data;
        }

        if (!data.HasValue)
            throw new InvalidOperationException("No data set.");

        return data.Value;
    }

    public void SetData(ChunkedEffectsCollection.ExportedData data)
    {
        lock (dataLock)
        {
            this.data = data;
        }
    }
}
