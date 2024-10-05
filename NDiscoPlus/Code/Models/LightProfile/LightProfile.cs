using NDiscoPlus.Code.LightHandlers;
using NDiscoPlus.Code.LightHandlers.Screen;
using NDiscoPlus.Shared.Effects.API.Channels.Effects.Intrinsics;
using NDiscoPlus.Shared.Helpers;
using NDiscoPlus.Shared.Models;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NDiscoPlus.Code.Models.LightProfile;

public sealed class LightProfile
{
    internal LightProfile(string name, IEnumerable<LightHandler> handlers, IEnumerable<KeyValuePair<LightId, LightConfig>> lightConfigurationOverrides)
    {
        Name = name;

        this.handlers = handlers.ToList();
        LightConfigurationOverrides = lightConfigurationOverrides.ToDictionary();
    }

    public string Name { get; set; } = string.Empty;

    public IList<LightHandler> Handlers => handlers.AsReadOnly();
    private readonly List<LightHandler> handlers;

    public Dictionary<LightId, LightConfig> LightConfigurationOverrides { get; }

    /// <summary>
    /// <para>First element in the array is the current profile.</para>
    /// <para>The rest of the values are unordered.</para>
    /// </summary>
    public static LightProfile[] LoadAll()
    {
        LightProfile[] profiles = Settings.LightProfiles.GetProfiles();

        if (profiles.Length < 1)
        {
            LightProfile defaultProfile = CreateDefault();
            profiles = [defaultProfile];
            SaveAll(profiles, defaultProfile);
        }

        return profiles;
    }
    public static LightProfile LoadCurrent() => LoadAll()[0];

    public static LightProfile CreateDefault()
    {
        return new LightProfile(
            name: string.Empty,
            handlers: [new ScreenLightHandler(null)],
            lightConfigurationOverrides: ImmutableDictionary<LightId, LightConfig>.Empty
        );
    }

    public static void SaveAll(LightProfile[] allProfiles, LightProfile currentProfile)
    {
        LightProfile[] newProfiles;

        int currentProfileIndex = Array.FindIndex(allProfiles, p => ReferenceEquals(p, currentProfile));
        if (currentProfileIndex != 0)
        {
            // copy profiles
            newProfiles = new LightProfile[allProfiles.Length];
            Array.Copy(allProfiles, newProfiles, allProfiles.Length);

            // switch places with current profile and first profile
            // (current profile moves to top of array)
            newProfiles[currentProfileIndex] = newProfiles[0];
            newProfiles[0] = currentProfile;
        }
        else
        {
            newProfiles = allProfiles;
        }

        Settings.LightProfiles.SetProfiles(newProfiles);
    }

    public bool CanAddHandler(LightHandlerImplementation implementation)
    {
        int existingCount = handlers.Count(h => h.GetType() == implementation.Type);
        return existingCount < implementation.MaxCount;
    }
    public bool CanAddHandler(Type type)
        => CanAddHandler(LightHandler.GetImplementation(type));
    public bool CanAddHandler<T>() where T : LightHandler
        => CanAddHandler(typeof(T));

    public bool CanRemoveHandler(LightHandlerImplementation implementation)
    {
        int existingCount = handlers.Count(h => h.GetType() == implementation.Type);
        return existingCount > implementation.MinCount;
    }
    public bool CanRemoveHandler(Type type)
        => CanRemoveHandler(LightHandler.GetImplementation(type));
    public bool CanRemoveHandler<T>() where T : LightHandler
        => CanRemoveHandler(typeof(T));

    /// <summary>
    /// Returns <see langword="false"/> if maximum handlers has already been reached.
    /// </summary>
    public bool TryAddHandler(LightHandlerImplementation implementation)
    {
        if (!CanAddHandler(implementation))
            return false;

        handlers.Add(implementation.CreateInstance(null));
        return true;
    }
    public bool TryAddHandler(Type type)
        => TryAddHandler(LightHandler.GetImplementation(type));
    public bool TryAddHandler<T>() where T : LightHandler
        => TryAddHandler(typeof(T));

    /// <summary>
    /// Returns <see langword="false"/> if minimum handlers has already been reached.
    /// </summary>
    public bool TryRemoveHandler(LightHandler handler)
    {
        if (!CanRemoveHandler(handler.GetType()))
            return false;

        handlers.Remove(handler);
        return true;
    }

    public IEnumerable<LightRecord> BuildLightRecords(IEnumerable<NDPLight> lights)
    {
        foreach (NDPLight light in lights)
        {
            if (LightConfigurationOverrides.TryGetValue(light.Id, out LightConfig? config))
                yield return config.CreateRecord(light);
            else
                yield return LightRecord.CreateDefault(light);
        }
    }
}