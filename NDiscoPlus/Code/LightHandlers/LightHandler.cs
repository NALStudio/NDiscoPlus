using MudBlazor;
using NDiscoPlus.Code.LightHandlers.Hue;
using NDiscoPlus.Code.LightHandlers.Screen;
using NDiscoPlus.Shared.Models;
using NDiscoPlus.Shared.Models.Color;
using NDiscoPlus.Shared.Music;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace NDiscoPlus.Code.LightHandlers;

public readonly record struct LightHandlerImplementation(
    Type Type,
    string DisplayName,
    string DisplayIcon
)
{
    /// <summary>
    /// Inclusive. (example: MinCount 1, LightHandler cannot be removed if it is the only handler remaining)
    /// </summary>
    public int MinCount { get; init; } = 0;

    /// <summary>
    /// Inclusive. (example: MinCount 1, LightHandler cannot be added if a handler already exists)
    /// </summary>
    public int MaxCount { get; init; } = 3;

    public LightHandler CreateInstance(LightHandlerConfig? config)
    {
        return (LightHandler)Activator.CreateInstance(Type, config)!;
    }
}

public abstract class LightHandler : IAsyncDisposable
{
    public static readonly ImmutableArray<LightHandlerImplementation> Implementations = [
        new(
            typeof(ScreenLightHandler),
            "Screen",
            Icons.Material.Rounded.DesktopWindows
        )
        {
            MaxCount = 1
        },
        new(
            typeof(HueLightHandler),
            "Philips Hue",
            Icons.Material.Rounded.Lightbulb
        )
    ];
    // Use FrozenDictionary as this is only instantiated once.
    private static readonly FrozenDictionary<Type, LightHandlerImplementation> implementationLookup = Implementations.ToFrozenDictionary(key => key.Type);

    public LightHandlerConfig Config { get; }

    public LightHandlerImplementation Implementation => GetImplementation(GetType());

    public static LightHandlerImplementation GetImplementation<T>() => GetImplementation(typeof(T));
    public static LightHandlerImplementation GetImplementation(Type type)
    {
        if (implementationLookup.TryGetValue(type, out LightHandlerImplementation impl))
            return impl;
        throw new ArgumentException($"Type not registered as a light handler implementation: {type.Name}", nameof(type));
    }

    protected LightHandler(LightHandlerConfig? config)
    {
        Config = config ?? CreateConfig();
    }

    /// <summary>
    /// This method is called by the constructor to create a default config instance for the object if none was passed to the constructor.
    /// </summary>
    protected abstract LightHandlerConfig CreateConfig();

    public abstract ValueTask<bool> ValidateConfig(ErrorMessageCollector? errors);

    public abstract IAsyncEnumerable<NDPLight> GetLights();

    /// <summary>
    /// <para>Start the handler if possible. </para>
    /// <para>If handler is already running, should result in a no-op.</para>
    /// </summary>
    public abstract ValueTask<bool> Start(ErrorMessageCollector? errors, out NDPLight[] lights);
    public abstract ValueTask Update(LightColorCollection lights);

    /// <summary>
    /// If handler isn't running, should result in a no-op.
    /// </summary>
    public abstract ValueTask Stop();

    public ValueTask DisposeAsync()
    {
        ValueTask t = Stop();
        GC.SuppressFinalize(this);
        return t;
    }
}

public abstract class LightHandler<T> : LightHandler where T : LightHandlerConfig
{
    protected LightHandler(T? config) : base(config)
    {
    }

    public new T Config => (T)base.Config;
    protected abstract override T CreateConfig();
}