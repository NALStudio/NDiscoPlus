using NDiscoPlus.Code.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.Code;
internal static partial class Settings
{
    internal static class Authentication
    {
        private const string _kSpotifyRefreshToken = "spotify-refresh-token";

        public static async Task<StoredSpotifyRefreshToken?> GetSpotifyRefreshToken()
        {
            string? refreshToken = await SecureStorage.GetAsync(_kSpotifyRefreshToken);
            return StoredSpotifyRefreshToken.DeserializeOrNull(refreshToken);
        }

        public static Task SetSpotifyRefreshToken(StoredSpotifyRefreshToken refreshToken)
        {
            string serialized = StoredSpotifyRefreshToken.Serialize(refreshToken);
            return SecureStorage.SetAsync(_kSpotifyRefreshToken, serialized);
        }
    }
}
