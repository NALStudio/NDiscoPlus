using Microsoft.JSInterop;
using NDiscoPlus.Shared.Helpers;
using NDiscoPlus.Shared.Models.Color;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Versioning;
using System.Text;

namespace NDiscoPlus.Components.Elements;

public partial class ScreenLightSignaler : IDisposable
{
    public static TimeSpan AnimationDuration => TimeSpan.FromSeconds(Animation.ForwardsSeconds + Animation.BackwardsSeconds);

    private class Animation
    {
        public const double ForwardsSeconds = 1;
        public const double BackwardsSeconds = 1;

        private double elapsedTime = 0d;

        public required double PosX;
        public required double PosY;
        public required string HTMLColor { get; init; }

        public double Progress { get; private set; }
        public bool IsFinished => elapsedTime >= (ForwardsSeconds + BackwardsSeconds);

        public void Update(double deltaTime)
        {
            elapsedTime += deltaTime;

            double newProgress;
            if (elapsedTime < ForwardsSeconds)
            {
                // Ease forwards (from 0 to 1)
                newProgress = Ease(elapsedTime / ForwardsSeconds);
            }
            else
            {
                double backwardsProgress = (elapsedTime - ForwardsSeconds) / BackwardsSeconds;
                // Ease backwards (from 1 to 0)
                newProgress = Ease(1d - backwardsProgress);
            }

            Progress = newProgress;
        }

        // https://easings.net/#easeOutCubic
        private static double Ease(double t)
        {
            double a = 1 - t;
            double b = a * a * a;
            return 1 - b;
        }
    }

    private readonly List<Animation> animations = new();
    private Task? updateAnimationsTask = null;
    private readonly CancellationTokenSource updateAnimationCancellation = new();

    private async Task UpdateAnimations(CancellationToken cancellationToken)
    {
        Stopwatch? sw = new();

        // 60 fps timer
        using PeriodicTimer pt = new(TimeSpan.FromSeconds(1d / 60d));
        while (animations.Count > 0 && await pt.WaitForNextTickAsync(cancellationToken))
        {
            double deltaTime = sw.Elapsed.TotalSeconds;
            sw.Restart();

            // Update animations
            foreach (Animation anim in animations)
                anim.Update(deltaTime);

            // Remove finished animations
            animations.RemoveAll(static anim => anim.IsFinished);

            StateHasChanged();
        }
    }

    public IEnumerable<string> ConstructCSS()
    {
        foreach (Animation anim in animations)
            yield return InternalConstructCSS(anim);
    }

    private static string InternalConstructCSS(Animation anim)
    {
        // percentage values
        string x = (anim.PosX * 100).ToString(CultureInfo.InvariantCulture);
        string y = (anim.PosY * 100).ToString(CultureInfo.InvariantCulture);
        string radius = (anim.Progress * 50).ToString(CultureInfo.InvariantCulture);

        // css
        return $"""
                position:fixed;
                top:0;
                bottom:0;
                left:0;
                right:0;
                z-index:1000000;
                background: radial-gradient(
                    circle {radius}vmin at {x}% {y}%,
                    {anim.HTMLColor}ff,
                    {anim.HTMLColor}00 100%
                );
                """;
    }

    public void Signal(double x, double y, NDPColor color)
    {
        (double R, double G, double B) rgb = color.ToSRGB();
        animations.Add(
            new Animation()
            {
                PosX = x,
                PosY = y,
                HTMLColor = ColorHelpers.ToHTMLColorRGB(color.ToSRGB())
            }
        );

        if (updateAnimationsTask?.IsCompleted != false)
            updateAnimationsTask = UpdateAnimations(updateAnimationCancellation.Token);
    }

    public void Dispose()
    {
        updateAnimationCancellation.Cancel();
        updateAnimationCancellation.Dispose();

        GC.SuppressFinalize(this);
    }
}