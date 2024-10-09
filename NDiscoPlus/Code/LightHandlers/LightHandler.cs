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
    string DisplayIcon,
    string DisplayHTMLColor
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
            Icons.Material.Rounded.DesktopWindows,
            MudBlazor.Colors.Cyan.Default
        )
        {
            MaxCount = 1
        },
        new(
            typeof(HueLightHandler),
            "Philips Hue",
            Icons.Material.Rounded.Lightbulb,
            MudBlazor.Colors.Pink.Default
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

    /// <summary>
    /// Get the lights of this handler with the current configuration.
    /// </summary>
    /// <returns>An async enumerable of the lights or an empty enumerable on error.</returns>
    public abstract IAsyncEnumerable<NDPLight> GetLights();

    /// <summary>
    /// <para>Start the handler if possible. </para>
    /// <para>If handler is already running, should result in an error <i>(not exception!)</i>.</para>
    /// </summary>
    /// <returns>The lights that are ready to receive updates or <see langword="null"/> if handler could not be started.</returns>
    public abstract ValueTask<NDPLight[]?> Start(ErrorMessageCollector? errors);
    public abstract ValueTask Update(LightColorCollection lights);

    /// <summary>
    /// <para>If handler isn't running, should result in a no-op.</para>
    /// <para>Stop is guaranteed to be called at least once which means it can be used to safely dispose any disposable resources.</para>
    /// </summary>
    public abstract ValueTask Stop();

    /// <summary>
    /// Flash the specified light with the provided color.
    /// </summary>
    /// <returns>The length of the signal or <see langword="null"/> if unsuccessful (config not valid, etc.)</returns>
    public abstract ValueTask<TimeSpan?> Signal(LightId lightId, NDPColor color);

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