using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Helpers;
internal static class HexConverter
{
    // Inspired from: https://stackoverflow.com/questions/321370/how-can-i-convert-a-hex-string-to-a-byte-array/9995303#9995303
    // and adapted to modern .NET standard practices (like use spans, remove bitshifts and use byte.Parse with spans)
    public static byte[] DecodeHex(scoped ReadOnlySpan<char> hex)
    {
        (int quotient, int remainder) = Math.DivRem(hex.Length, 2);
        if (remainder != 0)
            throw new ArgumentException("Hex string must have an even amount of digits.");

        byte[] output = new byte[quotient];

        Span<byte> outputSpan = output.AsSpan();
        for (int i = 0; i < outputSpan.Length; i++)
        {
            ReadOnlySpan<char> hexVal = hex.Slice(i * 2, 2);
            outputSpan[i] = byte.Parse(hexVal, System.Globalization.NumberStyles.HexNumber);
        }

        return output;
    }
}
