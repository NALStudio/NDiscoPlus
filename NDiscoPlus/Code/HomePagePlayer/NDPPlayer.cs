﻿using Microsoft.Extensions.Logging;
using NDiscoPlus.Code.LightHandlers;
using NDiscoPlus.Shared;
using NDiscoPlus.Shared.Models;
using NDiscoPlus.Spotify.Models;
using NDiscoPlus.Spotify.Players;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.Code.HomePagePlayer;
internal partial class NDPPlayer : IDisposable
{
    private const int _kTargetFps = 75;

    private readonly SpotifyPlayer spotify;
    private readonly PlayerData player;
    private readonly LightInterpreter lightInterpreter;

    private readonly object lightsLock = new();
    private LightData? lights;

    private readonly object sharedLock = new();
    private SharedData shared;
    public SharedData Data
    {
        get
        {
            lock (sharedLock)
            {
                return shared;
            }
        }
    }

    public NDPPlayer(SpotifyClient spotify, ILogger<SpotifyWebPlayer> playerLogger)
    {
        this.spotify = new SpotifyWebPlayer(spotify, logger: playerLogger);
        player = new(spotify);

        lightInterpreter = new();
    }

    public async Task Run(CancellationToken cancellationToken = default)
    {
        Task t = new(
            async () => await InternalRun(cancellationToken),
            cancellationToken,
            TaskCreationOptions.LongRunning
        );
        t.Start();

        await t;
    }

    public void StartLights(LightData lights)
    {
        lock (lightsLock)
        {
            this.lights = lights;
        }
    }
    public LightData? StopLights()
    {
        LightData? lights;

        lock (lightsLock)
        {
            lights = this.lights;
            this.lights = null;
        }

        return lights;
    }

    private async Task InternalRun(CancellationToken cancellationToken = default)
    {
        await foreach (SpotifyPlayerContext? context in spotify.ListenAsync(_kTargetFps, cancellationToken: cancellationToken))
            UpdateContext(context);
    }

    // Extracted into a separate function to make sure it isn't async
    // as our update loop is ran at 75 fps
    private void UpdateContext(SpotifyPlayerContext? ctx)
    {
        UpdateTrackData(current: ctx?.Track, next: ctx?.NextTrack);
        UpdateLights(ctx);
        SetSharedData(ctx);
    }

    private void UpdateLights(SpotifyPlayerContext? ctx)
    {
        LightData? lightsData;
        lock (lightsLock)
        {
            lightsData = this.lights;
        }
        if (lightsData is not LightData lights)
            return;


        NDPData? data = player.Current.RequestData(lights.Lights);
        _ = player.Next.RequestData(lights.Lights); // pre-request data for next track

        LightColorCollection lightColors;
        if (ctx is not null && data is not null)
            lightColors = RunInterpreter(ctx, data);
        else
            lightColors = LightColorCollection.Black(lights.Lights);

        foreach (LightHandler lh in lights.Handlers)
            lh.Update(lightColors);
    }

    private LightColorCollection RunInterpreter(SpotifyPlayerContext ctx, NDPData data)
    {
        LightInterpreterResult result = lightInterpreter.Update(ctx.Progress, data);
        // TODO: log fps
        return result.Lights;
    }

    private void SetSharedData(SpotifyPlayerContext? ctx)
    {
        NDPColorPalette? palette = player.Current.RequestPalette();
        _ = player.Next.RequestPalette(); // pre-request palette for next track

        lock (sharedLock)
        {
            shared = new(ctx, palette);
        }
    }

    private void UpdateTrackData(SpotifyPlayerTrack? current, SpotifyPlayerTrack? next)
    {
        if (player.Current.Track?.Id != current?.Id)
        {
            if (player.Current.Track?.Id == player.Next.Track?.Id)
                player.Current.CopyFrom(player.Next);
            else
                player.Current.Reset(current);
        }

        if (player.Next.Track?.Id != next?.Id)
            player.Next.Reset(next);
    }

    public void Dispose()
    {
        spotify.Dispose();
    }
}
