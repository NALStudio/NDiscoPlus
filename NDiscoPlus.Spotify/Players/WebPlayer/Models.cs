using NDiscoPlus.Spotify.Models;
using SpotifyAPI.Web;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.Spotify.Players.WebPlayer;
public partial class NewSpotifyWebPlayer
{
    private enum Availability { Available, DoesNotUpdate, NotAvailable }

    private class PlaybackState
    {
        public required SpotifyPlayerTrack Track { get; init; }

        public required bool IsPlaying { get; init; }
        public required long LastEditedTimestampMs { get; init; }
        public required int ProgressMs { get; init; }
        public long ProgressTicks => ProgressMs * TimeConversions.TicksPerMs;
    }

    private class Request<T>
    {
        public long SentTimestamp { get; }
        public long ReceivedTimestamp { get; }

        public T Value { get; }

        public Request(long sentTimestamp, long receivedTimestamp, T value)
        {
            SentTimestamp = sentTimestamp;
            ReceivedTimestamp = receivedTimestamp;

            Value = value;
        }
    }
}
