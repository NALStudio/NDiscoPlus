namespace NDiscoPlus.Shared.Models;

public class ScreenLightId : LightId
{
    public byte TotalLightCount { get; }
    public byte Index { get; }

    public override string HumanReadableString => $"Screen Light (count: {TotalLightCount}, index: {Index})";

    public ScreenLightId(byte totalLightCount, byte index)
    {
        TotalLightCount = totalLightCount;
        Index = index;
    }

    public override int GetHashCode()
        => HashCode.Combine(GetType(), TotalLightCount, Index);

    public override bool Equals(object? obj)
        => obj is ScreenLightId sli && TotalLightCount == sli.TotalLightCount && Index == sli.Index;
}
