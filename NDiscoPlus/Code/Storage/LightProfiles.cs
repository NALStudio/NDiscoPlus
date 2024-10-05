using NDiscoPlus.Code.Models.LightProfile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.Code;

internal static partial class Settings
{
    internal static class LightProfiles
    {
        private const string _kLightProfiles = "light-profiles";

        public static LightProfile[] GetProfiles()
        {
            SerializableLightProfile[]? ser = PreferencesHelpers.GetJsonArray<SerializableLightProfile>(_kLightProfiles);
            if (ser is null)
                return Array.Empty<LightProfile>();
            else
                return ser.Select(static s => SerializableLightProfile.Deconstruct(s)).ToArray();
        }

        public static void SetProfiles(LightProfile[] lightProfiles)
        {
            SerializableLightProfile[] ser = lightProfiles.Select(static s => SerializableLightProfile.Construct(s)).ToArray();
            PreferencesHelpers.SetJsonArray(_kLightProfiles, ser);
        }
    }
}
