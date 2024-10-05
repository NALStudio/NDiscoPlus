using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.Shared.Helpers;
public static class BitResolution
{
    private static class ConversionConstants
    {
        public static readonly double DoubleToUInt8 = GetDoubleConversion(byte.MaxValue);
        public static readonly double DoubleToUInt16 = GetDoubleConversion(ushort.MaxValue);
        public static readonly double DoubleToUInt32 = GetDoubleConversion(uint.MaxValue);

        private static double GetDoubleConversion(ulong maxValue)
        {
            if (maxValue >= ulong.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(maxValue), "maxValue must be less than ulong.MaxValue!");

            ulong incremented = maxValue + 1;
            double output = Math.BitDecrement((double)incremented);

            Debug.Assert(output < incremented);
            Debug.Assert(output > maxValue);
            return output;
        }
    }

    private static void ThrowIfNotBetween01<T>(T value, [CallerArgumentExpression(nameof(value))] string? paramName = null) where T : IFloatingPointIeee754<T>
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(value, T.Zero, paramName);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value, T.One, paramName);
    }

    public static byte AsUInt8(double value)
    {
        ThrowIfNotBetween01(value);
        return (byte)(value * ConversionConstants.DoubleToUInt8);
    }

    public static ushort AsUInt16(double value)
    {
        ThrowIfNotBetween01(value);
        return (ushort)(value * ConversionConstants.DoubleToUInt16);
    }

    public static uint AsUInt32(double value)
    {
        ThrowIfNotBetween01(value);
        return (uint)(value * ConversionConstants.DoubleToUInt32);
    }
}
