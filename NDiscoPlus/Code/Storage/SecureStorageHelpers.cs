using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NDiscoPlus.Code;
internal static partial class Settings
{
    private static class SecureStorageHelpers
    {
        public static async Task SetJson<T>(string key, T value)
        {
            string json = JsonSerializer.Serialize(value);
            await SecureStorage.SetAsync(key, json);
        }

        public static async Task<T?> GetJson<T>(string key)
        {
            string? json = await SecureStorage.GetAsync(key);
            if (json is null)
                return default;
            return JsonSerializer.Deserialize<T>(json);
        }

        public static async ValueTask SetNullable(string key, string? value)
        {
            if (value is not null)
                await SecureStorage.SetAsync(key, value);
            else
                _ = SecureStorage.Remove(key);
        }
    }
}
