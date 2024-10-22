namespace NDiscoPlus.Shared.Effects.Effects;

internal static class EffectConstants
{
    // Bridge sends at 25 Hz, so fastest effect rate must be less than 12.5 Hz (per API docs), so effect duration must be less than (1 / 12.5 Hz => 0,08 s)
    // relevant documentation: https://developers.meethue.com/develop/hue-entertainment/hue-entertainment-api/#best-practices
    public const int MinEffectDurationMs = 80; // effect duration must be > MinEffectDuration
    public static readonly TimeSpan MinEffectDuration = TimeSpan.FromMilliseconds(MinEffectDurationMs);
}