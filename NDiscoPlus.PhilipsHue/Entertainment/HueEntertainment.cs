using NDiscoPlus.PhilipsHue.Api.HueApi;
using NDiscoPlus.PhilipsHue.Authentication.Models;
using NDiscoPlus.PhilipsHue.Entertainment.Connection;
using NDiscoPlus.PhilipsHue.Entertainment.Models;
using NDiscoPlus.PhilipsHue.Entertainment.Models.Channels;
using NDiscoPlus.PhilipsHue.Helpers;
using Org.BouncyCastle.Crypto.Tls;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Entertainment;

// Heavy reference: https://github.com/michielpost/Q42.HueApi/blob/e3d059128e11ee19a7cac9fd2265bee989ac4271/src/HueApi.Entertainment/StreamingHueClient.cs
public sealed class HueEntertainment : IDisposable
{
    private readonly IPAddress bridgeIp;
    private readonly HueCredentials credentials;

    public Guid EntertainmentConfiguration { get; }
    private readonly ImmutableArray<byte> entConfigBytes;

    private readonly LocalHueApi hueApi;

    private readonly Socket socket;

    [MemberNotNullWhen(true, nameof(dtls), nameof(udp))]
    private bool Connected { get; set; }

    private DtlsTransport? dtls;
    private UdpTransport? udp;
    private byte sequenceId = byte.MaxValue;

    private HueEntertainment(string bridgeIp, HueCredentials credentials, Guid entertainmentConfigurationId)
    {
        this.bridgeIp = IPAddress.Parse(bridgeIp);

        this.credentials = credentials;

        EntertainmentConfiguration = entertainmentConfigurationId;
        entConfigBytes = GetGuidPayloadBytes(entertainmentConfigurationId);

        hueApi = new(bridgeIp, credentials);

        socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(new IPEndPoint(IPAddress.Any, 0));
    }

    private static ImmutableArray<byte> GetGuidPayloadBytes(Guid guid)
    {
        string guidString = guid.ToString().ToLowerInvariant();
        byte[] bytes = Encoding.ASCII.GetBytes(guidString);
        return ImmutableCollectionsMarshal.AsImmutableArray(bytes);
    }

    public static async Task<HueEntertainment?> ConnectAsync(string bridgeIp, HueCredentials credentials, Guid entertainmentConfigurationId)
    {
        HueEntertainment entertainment = new(bridgeIp, credentials, entertainmentConfigurationId);
        try
        {
            await entertainment.InternalConnectAsync();
        }
        catch
        {
            entertainment.Dispose();
            return null;
        }

        return entertainment;
    }
    private async Task InternalConnectAsync()
    {
        await hueApi.UpdateEntertainmentConfigurationActionAsync(EntertainmentConfiguration, "start");

        BasicTlsPskIdentity pskIdentity = new(credentials.AppKey, HexConverter.DecodeHex(credentials.ClientKey));

        DtlsClient dtlsClient = new(null, pskIdentity);

        DtlsClientProtocol clientProtocol = new(new SecureRandom());
        await socket.ConnectAsync(bridgeIp, 2100);

        udp = new UdpTransport(socket);
        dtls = clientProtocol.Connect(dtlsClient, udp);

        sequenceId = byte.MaxValue;

        Connected = true;
    }

    public void Send(IReadOnlyList<HueRGBEntertainmentChannel> channels) => Send(0x00, channels);
    public void Send(IReadOnlyList<HueXYEntertainmentChannel> channels) => Send(0x01, channels);

    private void Send<T>(byte colorSpace, IReadOnlyList<T> channels) where T : IHueEntertainmentChannel
    {
        unchecked // unchecked to allow to roll back to zero after 255
        {
            sequenceId++; // sequenceId is initialized with byte.MaxValue so the first value sent to the bridge will be 0
        }

        if (channels.Count > 20)
            throw new ArgumentException($"Maximum 20 slots of color data supported, got: {channels}");

        // Construct channel bytes
        const int kBytesPerChannel = IHueEntertainmentChannel.BytesPerChannel;
        Span<byte> channelBytes = stackalloc byte[channels.Count * kBytesPerChannel];
        for (int i = 0; i < channels.Count; i++)
        {
            Span<byte> channelSlice = channelBytes.Slice(i * kBytesPerChannel, kBytesPerChannel);
            channels[i].SetBytes(channelSlice);
        }

        // Construct message
        Debug.Assert(entConfigBytes.Length == 36);
        byte[] bytes = [
            0x48, 0x75, 0x65, 0x53, 0x74, 0x72, 0x65, 0x61, 0x6d, // 'H', 'u', 'e', 'S', 't', 'r', 'e', 'a', 'm'
            0x02, 0x00, // version 2.0
            sequenceId,
            0x00, 0x00, // reserved
            colorSpace,
            0x00, // reserved
            ..entConfigBytes,
            ..channelBytes
        ];
        // low level C# uses list to build the bytes array and then converts to byte[]
        // which is kinda stupid as we already know the length of the array at runtime
        // but whatever, shouldn't be that much of a performance hit even though this is a hotpath that's called 100 times a second

        // Send message
        Send(bytes);
    }

    private void Send(byte[] data)
    {
        if (!Connected) // We should be connected as long as dispose isn't called
            throw new HueEntertainmentException("Entertainment API disposed.");

        const int offset = 0;
        int length = data.Length;
        dtls.Send(data, offset, length);
    }

    private void Disconnect()
    {
        Debug.Assert(Connected == socket.Connected);
        if (!Connected)
            return;

        Connected = false;

        dtls.Close();
        dtls = null;

        udp.Close();
        udp = null;

        // https://stackoverflow.com/questions/35229143/what-exactly-do-sockets-shutdown-disconnect-close-and-dispose-do/35229144#35229144
        // We shut down because we don't care whether the unsent packages ever make it to the bridge.
        socket.Shutdown(SocketShutdown.Both);
    }

    public void Dispose()
    {
        Disconnect();

        hueApi.Dispose();
        socket.Dispose();

        GC.SuppressFinalize(this);
    }
}
