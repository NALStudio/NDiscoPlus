namespace NDiscoPlus.Shared.Models;

public class HueLightId : LightId
{
    public Guid EntertainmentConfigurationId { get; }
    public byte ChannelId { get; }

    public override string HumanReadableString => $"Hue Light (config: {EntertainmentConfigurationId}, id: {ChannelId})";

    public HueLightId(Guid entertainmentConfigurationId, byte channelId)
    {
        EntertainmentConfigurationId = entertainmentConfigurationId;
        ChannelId = channelId;
    }

    public override int GetHashCode()
        => HashCode.Combine(GetType(), EntertainmentConfigurationId, ChannelId);

    public override bool Equals(object? obj)
        => obj is HueLightId hli && EntertainmentConfigurationId == hli.EntertainmentConfigurationId && ChannelId == hli.ChannelId;
}
