using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Entertainment.Models.Channels;
public interface IHueEntertainmentChannel
{
    public const int BytesPerChannel = 7;

    public abstract byte Id { get; }

    protected abstract ushort ColorChannel1 { get; }
    protected abstract ushort ColorChannel2 { get; }
    protected abstract ushort ColorChannel3 { get; }

    public sealed void SetBytes(scoped Span<byte> bytes)
    {
        if (bytes.Length != BytesPerChannel)
            throw new ArgumentException($"Expected {BytesPerChannel} bytes, got {bytes.Length} instead.");

        bytes[0] = Id;
        SetColor(bytes, 1, ColorChannel1);
        SetColor(bytes, 3, ColorChannel2);
        SetColor(bytes, 5, ColorChannel3);
    }

    private static void SetColor(scoped Span<byte> bytes, int offset, ushort color)
    {
        /* System.BitConverter implementation:
        public static byte[] GetBytes(ushort value)
        {
            byte[] bytes = new byte[sizeof(ushort)];
            Unsafe.As<byte, ushort>(ref bytes[0]) = value;
            return bytes;
        }
        */

        Unsafe.As<byte, ushort>(ref bytes[offset]) = color;
        if (BitConverter.IsLittleEndian)
            Swap(bytes, offset, offset + 1);
    }

    private static void Swap(scoped Span<byte> bytes, int element1, int element2)
        => (bytes[element2], bytes[element1]) = (bytes[element1], bytes[element2]);
}
