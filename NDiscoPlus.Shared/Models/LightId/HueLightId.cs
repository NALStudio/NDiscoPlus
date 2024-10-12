namespace NDiscoPlus.Shared.Models;

public class HueLightId : LightId
{
    public required Guid EntertainmentConfigurationId { get; init; }
    public required byte ChannelId { get; init; }

    // We don't care about LightId equality as it's not used during effect playing
    public required Guid? LightId { get; init; } // For signaling

    public override string HumanReadableString => $"Hue Light (channel: {ChannelId})";

    public override int GetHashCode()
        => HashCode.Combine(GetType(), EntertainmentConfigurationId, ChannelId);

    public override bool Equals(object? obj)
        => obj is HueLightId hli
        && EntertainmentConfigurationId == hli.EntertainmentConfigurationId
        && ChannelId == hli.ChannelId;
}
