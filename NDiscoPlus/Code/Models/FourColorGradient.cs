using NDiscoPlus.Shared.Models;
using NDiscoPlus.Shared.Models.Color;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.Code.Models;
internal readonly struct FourColorGradient
{
    public readonly ImmutableArray<NDPColor> Colors { get; }

    public FourColorGradient(ImmutableArray<NDPColor> colors)
    {
        Colors = colors;
    }

    public static FourColorGradient? TryCreateFromPalette(NDPColorPalette palette)
    {
        if (palette.Count < 4)
            return null;

        return new(palette.Take(4).ToImmutableArray());
    }
}
