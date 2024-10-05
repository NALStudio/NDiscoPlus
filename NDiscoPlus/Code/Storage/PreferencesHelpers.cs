using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NDiscoPlus.Code;
internal static partial class Settings
{
    private static class PreferencesHelpers
    {
        public static void SetJson<T>(string key, T value)
        {
            string json = JsonSerializer.Serialize(value);
            Preferences.Set(key, json);
        }

        public static T? GetJson<T>(string key)
        {
            string? json = Preferences.Get(key, null);
            if (json is null)
                return default;
            return JsonSerializer.Deserialize<T>(json);
        }

        private static string GetArrayLengthKey(string key) => key + "$length";
        private static string GetArrayElementKey(string key, int index) => key + "$" + index.ToString(CultureInfo.InvariantCulture);

        public static void SetJsonArray<T>(string key, T[] array)
        {
            int length = array.Length;

            // Remove items that go out of bounds of the new array size
            int oldLength = Preferences.Get(GetArrayLengthKey(key), -1);
            while (oldLength > length)
            {
                oldLength--;
                Preferences.Remove(GetArrayElementKey(key, oldLength)); // after decrement, oldLength == last item index
            }

            // Set new values overriding the previous values if any exist
            Preferences.Set(GetArrayLengthKey(key), length);
            for (int i = 0; i < length; i++)
            {
                string json = JsonSerializer.Serialize(array[i]);
                Preferences.Set(GetArrayElementKey(key, i), json);
            }
        }

        /// <summary>
        /// <para>Returns an empty array if missing.</para>
        /// <para>Returns an empty array if any of the values could not be deserialized.</para>
        /// </summary>
        public static T[]? GetJsonArray<T>(string key)
        {
            int length = Preferences.Get(GetArrayLengthKey(key), -1);
            if (length < 0)
                return null;
            if (length == 0)
                return Array.Empty<T>();

            T[] values = new T[length];
            for (int i = 0; i < length; i++)
            {
                string? json = Preferences.Get(GetArrayElementKey(key, i), null);
                if (json is null)
                    return null;

                T? element = JsonSerializer.Deserialize<T>(json);
                if (element is null)
                    return null;

                values[i] = element;
            }

            return values;
        }
    }
}
