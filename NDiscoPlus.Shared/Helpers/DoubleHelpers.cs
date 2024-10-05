using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.Shared.Helpers;
public static class DoubleHelpers
{
    public static double Remap(this double value, double from1, double to1, double from2, double to2)
        => (value - from1) / (to1 - from1) * (to2 - from2) + from2;

    public static double Remap(this double value, (double From, double To) from, (double From, double To) to)
        => Remap(value, from.From, from.To, to.From, to.To);

    public static double Remap01(this double value, double from1, double to1)
        => (value - from1) / (to1 - from1);

    public static double Clamp(this double value, double min, double max)
        => Math.Clamp(value, min, max);

    public static double Clamp01(this double value)
         => Math.Clamp(value, 0d, 1d);

    public static double Lerp(double a, double b, double t)
        => a + (b - a) * Clamp01(t);

    public static double LerpUnclamped(double a, double b, double t)
        => a + (b - a) * t;

    public static double InverseLerp(double a, double b, double value)
    {
        if (a != b)
            return Math.Clamp((value - a) / (b - a), 0d, 1d);
        else
            return 0.0;
    }
}
